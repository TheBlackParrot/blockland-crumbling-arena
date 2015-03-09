exec("./support.cs");
exec("./system.cs");
exec("./board_functions.cs");
exec("./player_functions.cs");

datablock AudioProfile(ping1)
{
	filename = "./sounds/ping1.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(ping2:ping1) { filename = "./sounds/ping2.wav"; };
datablock AudioProfile(ping3:ping1) { filename = "./sounds/ping3.wav"; };
datablock AudioProfile(gameStart:ping1) { filename = "./sounds/gameStart-Post.wav"; };
datablock AudioProfile(gameStartPre:ping1) { filename = "./sounds/gameStart-Pre.wav"; };
datablock AudioProfile(gameEnd:ping1) { filename = "./sounds/gameEnd.wav"; };

package CrumblePackage {
	function MinigameSO::reset(%this) {
		if(!$Crumbling::BuildingBoard && $Crumbling::HasStarted) {
			%this.startedGameAt = 0;
			startNewBoard();
			return parent::reset(%this);
		}
		// inital server startup
		if(!$Crumbling::HasInit) {
			$Crumbling::HasInit = 1;
			startNewBoard();
			return parent::reset(%this);
		}
		$Crumbling::HasStarted = 1;
		%this.respawnAll();
		%this.onGoing = 0;
		cancel(%this.cheatTick);
		cancel(%this.countdownLoop);
		cancel(%this.resetSched);
		cancel($Crumbling::BuildSched);
		%this.startGame();
		return parent::reset(%this);
	}

	function MinigameSO::checkLastManStanding(%this) {
		if($Crumbling::BuildingBoard) {
			return;
		}
		%count = 0;
		for(%i=0;%i<%this.numMembers;%i++) {
			%client = %this.member[%i];
			if(isObject(%client.player)) {
				if(%client.player.getState() !$= "Dead") {
					%count++;
					%selected = %client.player;
				}
			}
		}
		if(%count == 1) {
			%this.playSound(gameEnd);
			%selected.canBreak = 0;
			cancel(%this.cheatTick);
			cancel(%this.countdownLoop);
			cancel(%this.resetSched);
			cancel($Crumbling::BuildSched);
		}
		return parent::checkLastManStanding(%this);
	}

	function GameConnection::spawnPlayer(%this) {
		if($Crumbling::BuildingBoard) {
			return;
		}
		parent::spawnPlayer(%this);
		%this.player.setTransform(ArenaSpawnPoints.getObject(getRandom(0,ArenaSpawnPoints.getCount()-1)).position);

		if(!isEventPending(%this.statsLoop)) {
			%this.doBottomStats();
		}
	}

	function fxDTSBrick::onPlayerTouch(%this,%player) {
		%player.lastTouched = getSimTime();
		if(%player.canBreak && !isEventPending(%this.breakSched)) {
			%this.breakBrick();
		}
		return parent::onPlayerTouch(%this,%player);
	}

	function fxDTSBrick::breakBrick(%this) {
		%this.setColorFX(3);
		%this.breakSched = %this.schedule(300,disappear,10);
		%this.soundSched = %this.schedule(300,playSound,brickPlantSound);
		%this.deleteSched = %this.schedule(1000,delete);
	}

	function PlayerStandardArmor::onEnterLiquid(%data,%obj,%coverage,%type) {
		%obj.kill();
		return parent::onEnterLiquid(%data,%obj,%coverage,%type);
	}

	function onServerDestroyed() {
		%mg = $DefaultMinigame;
		cancel(%mg.cheatTick);
		cancel(%mg.countdownLoop);
		cancel(%mg.resetSched);
		cancel($Crumbling::BuildSched);
		return parent::onServerDestroyed();
	}
};
activatePackage(CrumblePackage);