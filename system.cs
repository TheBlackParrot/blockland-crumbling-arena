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