function GameConnection::doBottomStats(%this) {
	cancel(%this.statsLoop);
	%this.statsLoop = %this.schedule(1000,doBottomStats);

	if(!$DefaultMinigame.startedGameAt) {
		%time = 0;
	} else {
		%time = mFloor((getSimTime() - $DefaultMinigame.startedGameAt)/1000);
	}

	if($Crumbling::BuildingBoard) {
		%percent = "N/A";
	} else {
		%percent = mFloor((BrickGroup_888888.getCount()/$DefaultMinigame.startingBricks)*100) @ "%";
	}
	%this.bottomPrint("<font:Arial Bold:14>\c3Game Time\c6:" SPC getTimeString(%time) @ "  \c3Bricks Remaining\c6:" SPC BrickGroup_888888.getCount() SPC "[" @ %percent @ "]",2,1);
}