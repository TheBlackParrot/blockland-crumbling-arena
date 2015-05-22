//gravityRocketProjectile
function spawnRockets(%amount) {
	for(%i=0;%i<%amount;%i++) {
		%pos = ArenaSpawnPoints.getObject(getRandom(ArenaSpawnPoints.getCount()-1)).position;
		%pos[xy] = getWords(%pos,0,1);
		%pos[z] = getWord(%pos,2) + 100;

		schedule(100*%i,0,spawnRocket,%pos[xy] SPC %pos[z]);

	}
}

function spawnRocket(%pos) {
	//.30 to 1.50
	%scale = getRandom(20,60)/100;
	%scale_vec = %scale SPC %scale SPC %scale;
	%proj = new Projectile() {
		dataBlock = gravityRocketProjectile;
		initialPosition = %pos;
		initialVelocity = "0 0" SPC -25/%scale;
		sourceObject = "";
		client = "";
		sourceSlot = 0;
		originPoint = %pos;
		scale = %scale_vec;
	};
	MissionCleanup.add(%proj);
	gameDebugMessage("Rockets","Spawned rocket at" SPC %pos SPC "with a scale of x" @ %scale @ ", class" SPC %proj.getClassName());
	//%proj.dump();
}

function triggerSurroundingBricks(%pos,%scale) {
	initContainerRadiusSearch(%pos,%scale*5,$TypeMasks::FXBrickObjectType);
	while(%obj = containerSearchNext()) {
		if(!isEventPending(%obj.breakSched)) {
			%obj.breakBrick();
		}
	}
}

package CrumblingRocketPackage {
	function ProjectileData::onExplode(%this,%obj,%pos,%c) {
		gameDebugMessage("Core","Deleted projectile" SPC %obj @ ", class" SPC %obj.getClassName());
		if(%obj.getDatablock().getName() $= "gravityRocketProjectile") {
			triggerSurroundingBricks(%obj.getPosition(),%obj.scale);
			gameDebugMessage("Rockets","Triggered surrounding check at" SPC %obj.getPosition());
		}
		parent::onExplode(%this,%obj,%pos,%c);
	}
};
activatePackage(CrumblingRocketPackage);