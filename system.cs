function MinigameSO::startGame(%this) {
	if(%this.onGoing) {
		return;
	}
	%this.onGoing = 1;
	%this.startedGameAt = 0;
	%this.schedule(1000,doCountdown,9);
	%this.playSound(gameStartPre);
	if(!isEventPending(%this.cheatTick)) {
		%this.cheatLoop();
	}
}

function MinigameSO::cancelGame(%this,%delete) {
	%this.onGoing = 0;
	cancel(%this.cheatTick);
	cancel(%this.countdownLoop);
	cancel(%this.resetSched);
	cancel(%this.speedSched);
	cancel(%this.modifierLoopSched);
	cancel(%this.environmentSched);
	cancel(%this.lightsSched);
	cancel(%this.sizeSched);
	cancel($Crumbling::BuildSched);
	if(%delete) {
		for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
			%client = $DefaultMinigame.member[%i];
			if(isObject(%client.player)) {
				%client.player.delete();
				%camera = %client.Camera;
				%camera.setFlyMode();
				%camera.mode = "Observer";
				%client.setControlObject(%camera);
			}
		}
	}
}

function MinigameSO::speedLoop(%this) {
	cancel(%this.speedSched);
	if(%this.onGoing && BrickGroup_888888.getCount() > 200) {
		%this.speedSched = %this.schedule(mCeil(50000/BrickGroup_888888.getCount()),speedLoop);
	}
	if(!$Crumbling::AnnouncedSpeed) {
		messageAll('',"\c6Let's speed it up a bit!");
		$Crumbling::AnnouncedSpeed = 1;
	}

	%brick = BrickGroup_888888.getObject(getRandom(0,BrickGroup_888888.getCount()-1));
	while(!isObject(%brick)) {
		%brick = BrickGroup_888888.getObject(getRandom(0,BrickGroup_888888.getCount()-1));
	}
	while(isEventPending(%brick.breakSched)) {
		%brick = BrickGroup_888888.getObject(getRandom(0,BrickGroup_888888.getCount()-1));
	}

	%brick.breakBrick();
}

function MinigameSO::cheatLoop(%this) {
	cancel(%this.cheatTick);
	%this.cheatTick = %this.schedule(5000,cheatLoop);

	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			if(!%client.player.lastTouched) {
				continue;
			}
			if(getSimTime() - %client.player.lastTouched >= 30000) {
				messageClient(%client,'',"\c6You were killed via cheat prevention.");
				%client.player.kill();
			}
		}
	}
}

function MinigameSO::doCountdown(%this,%x) {
	cancel(%this.countdownLoop);

	if(!%x) {
		for(%i=0;%i<%this.numMembers;%i++) {
			%client = %this.member[%i];
			if(isObject(%client.player)) {
				%client.player.canBreak = 1;
			}
		}
		%this.centerPrintAll("<font:Arial Bold:48>\c1START!",3);
		$Crumbling::AnnouncedSpeed = 0;
		%this.speedSched = %this.schedule(120000,speedLoop);
		%this.modifierLoopSched = %this.schedule(30000,modifierLoop);
		%this.startedGameAt = getSimTime();
		%this.playSound(gameStart);
		return;
	} else {
		%this.countdownLoop = %this.schedule(1000,doCountdown,%x-1);
	}

	if(%x >= 3) { %this.playSound(ping1); }
	if(%x == 2) { %this.playSound(ping2); }
	if(%x <= 1) { %this.playSound(ping3); }
	%this.centerPrintAll("<font:Arial Bold:32>\c6The game will start in<br><font:Arial Bold:48>\c2" @ %x,2);
}

function MinigameSO::modifierLoop(%this) {
	cancel(%this.modifierLoopSched);
	%delay_modif = getRandom(-1500,1500);
	%this.modifierLoopSched = %this.schedule(30000+%delay_modif,modifierLoop);

	%item = getRandom(1,4);
	switch(%item) {
		// -- deprecated due to new environment system, seems a bit pointless now --
		//case 0:
		//	setEnvironmentTheme("dark");
		//	messageAll('',"\c4AI\c6: Oops, sorry, black hole.");
		//	%this.environmentSched = schedule(15000+%delay_modif,0,setEnvironmentTheme,"default");
		//	%this.toggleLights(1);
		//	%this.lightsSched = %this.schedule(15000+%delay_modif,toggleLights,0);
		case 1:
			if($Crumbling::BrickArea > 16) {
				messageAll('',"\c4AI\c6: Have a free pushbroom!");
				%this.giveItem("Push Broom",%delay_modif);
			} else {
				return;
			}
		case 2:
			messageAll('',"\c4AI\c6: Have a free sword!");
			%this.giveItem("Sword",%delay_modif);
		case 3:
			messageAll('',"\c4AI\c6: You all seem a bit, larger than normal...");
			%this.sizePlayers(2);
			%this.sizeSched = %this.schedule(15000+%delay_modif,sizePlayers,1);
		case 4:
			messageAll('',"\c4AI\c6: Sorry, I seemed to have shuffled you up...");
			%this.randomizePlayerPositions();
	}		
	serverPlay2D(lightOnSound);
}

function MinigameSO::randomizePlayerPositions(%this) {
	%count = 0;
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			%pos[%count] = %client.player.getTransform();
			%vel[%count] = %client.player.getVelocity();
			%player[%count] = %client.player;
			%count++;
		}
	}
	for(%i=0;%i<%count;%i++) {
		%new_pos = getRandom(0,%count-1);
		while(%selected[%new_pos]) {
			%new_pos = getRandom(0,%count-1);
		}
		%selected[%new_pos] = 1;
		%player[%i].setTransform(%pos[%new_pos]);
		%player[%i].setVelocity("0 0 0");
		%player[%i].addVelocity(%vel[%new_pos]);
	}
}

function MinigameSO::toggleLights(%this,%bool) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			if(!isObject(%client.player.light) && %bool) {
				serverCmdLight(%client);
			}
			if(isObject(%client.player.light) && !%bool) {
				serverCmdLight(%client);
			}
		}
	}
}

function MinigameSO::sizePlayers(%this,%size) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			%client.player.setPlayerScale(%size SPC %size SPC %size);
		}
	}
}

function MinigameSO::giveItem(%this,%item,%delay_modif) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			%client.player.addNewItem(%item);
			%client.player.schedule(15000+%delay_modif,clearTools);
		}
	}
}