$Crumbling::AcceptedBricks = "brick2x2Data brick2x2fData brick2x4Data brick2x4fData brick4x4Data brick4x4fData brick4x6Data brick4x8Data brick4x8fData brick8x8Data brick8x8fData brick4xCubeData brick10x10Data brick2x2x3Data";
if($Crumbling::Offset $= "") {
	$Crumbling::Offset = getRandom(-1000,1000) SPC getRandom(-1000,1000) SPC 100;
}

if(!isObject(ArenaSpawnPoints)) {
	new SimSet(ArenaSpawnPoints);
}

function startNewBoard() {
	%colors = getRandom(0,4);
	%brick = getWord($Crumbling::AcceptedBricks,getRandom(0,getWordCount($Crumbling::AcceptedBricks)-1));
	%x = getRandom(16,24)*(mCeil($DefaultMinigame.numMembers/2.6));
	%y = getRandom(16,24)*(mCeil($DefaultMinigame.numMembers/2.6));
	%z = getRandom(1,12);

	startBuildingBoard(%x SPC %y SPC %z,%colors,%brick);
	ArenaSpawnPoints.clear();
}
function startBuildingBoard(%size,%colors,%brick) {
	BrickGroup_888888.deleteAll();
	$Crumbling::BuildingBoard = 1;
	$Crumbling::BuildStart = getRealTime();
	messageAll('MsgUploadStart',"\c0Loading a" SPC getWord(%size,0) @ "x" @ getWord(%size,1) @ "x" @ getWord(%size,2) SPC %brick.uiName SPC "brick arena. Please wait...");
	buildBoard(0,0,0,%brick,getWord(%size,0),getWord(%size,1),getWord(%size,2)-1,%colors);
}

function buildBoard(%x,%y,%z,%brickdata,%maxx,%maxy,%maxz,%gradient) {
	%size[x] = %brickdata.brickSizeX/2;
	%size[y] = %brickdata.brickSizeY/2;
	%size[z] = %brickdata.brickSizeZ/5;

	%offset[x] = getWord($Crumbling::Offset,0);
	%offset[y] = getWord($Crumbling::Offset,1);
	%offset[z] = getWord($Crumbling::Offset,2);

	%pos[x] = (%x*%size[x])+%offset[x];
	%pos[y] = (%y*%size[y])+%offset[y];
	%pos[z] = (%z*%size[z])+%offset[z];

	%color = (%maxz-%z)+(12*%gradient);

	%brick = new fxDTSBrick(ArenaBrick) {
		angleID = 0;
		colorFxID = 0;
		colorID = %color;
		dataBlock = %brickdata;
		enableTouch = 1;
		isBasePlate = 0;
		isPlanted = 1;
		position = %pos[x] SPC %pos[y] SPC %pos[z];
		printID = 0;
		scale = "1 1 1";
		shapeFxID = 0;
		stackBL_ID = 888888;
	};
	if(%z == %maxz) {
		%spawn = new ScriptObject(ArenaSpawnPoint) {
			position = %pos[x] SPC %pos[y] SPC %pos[z]+%size[z];
		};
		ArenaSpawnPoints.add(%spawn);
	}
	BrickGroup_888888.add(%brick);
	%brick.plant();
	%brick.setTrusted(1);

	%x++;
	if(%z >= %maxz && %y >= %maxy && %x >= %maxx) {
		endBuildBoard();
		return;
	}
	if(%y >= %maxy && %x >= %maxx) {
		%x = 0;
		%y = 0;
		%z++;
	}
	if(%x >= %maxx) {
		%x = 0;
		%y++;
	}
	$Crumbling::BuildSched = schedule(2,0,buildBoard,%x,%y,%z,%brickdata,%maxx,%maxy,%maxz,%gradient);
}

function endBuildBoard() {
	%time = getRealTime() - $Crumbling::BuildStart;
	messageAll('MsgProcessComplete',"\c0" @ BrickGroup_888888.getCount() SPC "bricks generated in" SPC getTimeString(%time/1000));
	$Crumbling::BuildingBoard = 0;
	$Crumbling::HasStarted = 0;
	$DefaultMinigame.startingBricks = BrickGroup_888888.getCount();
	$DefaultMinigame.resetSched = $DefaultMinigame.schedule(2000,reset);
}