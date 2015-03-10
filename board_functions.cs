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
	%x = getRandom(12,20)*(mCeil($DefaultMinigame.numMembers/8));
	%y = getRandom(12,20)*(mCeil($DefaultMinigame.numMembers/8));
	%z = getRandom(1,12);
	%spaced = getRandom(0,8);

	startBuildingBoard(%x SPC %y SPC %z,%colors,%brick,%spaced);
	ArenaSpawnPoints.clear();
}
function startBuildingBoard(%size,%colors,%brick,%spaced) {
	BrickGroup_888888.deleteAll();
	$Crumbling::BuildingBoard = 1;
	$Crumbling::BuildStart = getRealTime();
	if(!%spaced) {
		%spacedstr = "an Expanded";
	} else {
		%spacedstr = "a";
	}
	messageAll('MsgUploadStart',"\c0Loading " @ %spacedstr SPC getWord(%size,0) @ "x" @ getWord(%size,1) @ "x" @ getWord(%size,2) SPC %brick.uiName SPC "brick arena. Please wait... \c7[ETA:" SPC getTimeString((getWord(%size,0)*getWord(%size,1)*getWord(%size,2)*2)/1000) @ "]");
	buildBoard(0,0,0,%brick,getWord(%size,0) SPC getWord(%size,1) SPC getWord(%size,2)-1,%colors,%spaced);


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

	$Crumbling::BrickArea = %brick.brickSizeX * %brick.brickSizeY;
}

function buildBoard(%x,%y,%z,%brickdata,%max,%gradient,%spaced) {
	%size[x] = %brickdata.brickSizeX/2;
	%size[y] = %brickdata.brickSizeY/2;
	%size[z] = %brickdata.brickSizeZ/5;

	%offset[x] = getWord($Crumbling::Offset,0);
	%offset[y] = getWord($Crumbling::Offset,1);
	%offset[z] = getWord($Crumbling::Offset,2);

	%pos[x] = (%x*%size[x])+%offset[x];
	%pos[y] = (%y*%size[y])+%offset[y];
	%pos[z] = (%z*%size[z])+%offset[z];

	%max[x] = getWord(%max,0);
	%max[y] = getWord(%max,1);
	%max[z] = getWord(%max,2);

	if(!%spaced) {
		%pos[z] += (%z*12)+%size[z];
	}

	%color = (%max[z]-%z)+(12*%gradient);

	%name = "ArenaBrick_" @ %x @ "_" @ %y @ "_" @ %z;

	%brick = new fxDTSBrick(%name) {
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
		rand = %ter_rand;
	};
	if(%z == %max[z]) {
		%spawn = new ScriptObject(ArenaSpawnPoint) {
			position = %pos[x] SPC %pos[y] SPC %pos[z]+%size[z];
		};
		ArenaSpawnPoints.add(%spawn);
	}
	BrickGroup_888888.add(%brick);
	%brick.plant();
	%brick.setTrusted(1);

	%x++;
	if(%z >= %max[z] && %y >= %max[y] && %x >= %max[x]) {
		endBuildBoard();
		return;
	}
	if(%y >= %max[y] && %x >= %max[x]) {
		%x = 0;
		%y = 0;
		%z++;
	}
	if(%x >= %max[x]) {
		%x = 0;
		%y++;
	}
	$Crumbling::BuildSched = schedule(2,0,buildBoard,%x,%y,%z,%brickdata,%max,%gradient,%spaced);
}

function endBuildBoard() {
	%time = getRealTime() - $Crumbling::BuildStart;
	messageAll('MsgProcessComplete',"\c0" @ BrickGroup_888888.getCount() SPC "bricks generated in" SPC getTimeString(%time/1000));
	$Crumbling::BuildingBoard = 0;
	$Crumbling::HasStarted = 0;
	$DefaultMinigame.startingBricks = BrickGroup_888888.getCount();
	$DefaultMinigame.resetSched = $DefaultMinigame.schedule(2000,reset);
}