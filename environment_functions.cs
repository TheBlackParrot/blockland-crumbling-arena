function EnvDB::addEnvironment(%this,%filename) {
	if(%filename $= "") {
		return;
	}

	%name = "Environment" @ %this.getCount();

	if(!isObject(%name)) {
		%obj = new ScriptObject(%name) {
			filename = %filename;
		};
		%this.add(%obj);
		gameDebugMessage("EnvDB","Added environment" SPC %filename);
	} else {
		gameDebugMessage("EnvDB","Environment" SPC %filename SPC "already exists","error");
	}
}

function EnvDB::setEnvironmentTheme(%this,%which) {
	if(%which > %this.getCount() || %which < 0) {
		gameDebugMessage("EnvDB","Attempted to add a non-existant environment" SPC %which,"error",1);
		return;
	}

	loadEnvironment(%this.getObject(%which).filename);
}

function initEnvDB() {
	if(!isObject(EnvDBGroup)) {
		new ScriptGroup(EnvDBGroup) {
			class = "EnvDB";
		};
	} else {
		while(EnvDBGroup.getCount() > 0) {
			EnvDBGroup.getObject(0).delete();
		}
	}

	%this = EnvDBGroup;

	%pattern = "Add-Ons/Gamemode_Crumbling_Arena_II/env/*";
	%file = findFirstFile(%pattern);
	while(%file !$= "") {
		%this.addEnvironment(%file);
		%file = findNextFile(%pattern);
	}
	%file = "";

	%pattern = "config/server/CA2/env/*";
	%file = findFirstFile(%pattern);
	while(%file !$= "") {
		%this.addEnvironment(%file);
		%file = findNextFile(%pattern);
	}

	if(!EnvDBGroup.getCount()) {
		gameDebugMessage("EnvDB","No environments were added!","error",1);
	} else {
		gameDebugMessage("EnvDB","Added" SPC EnvDBGroup.getCount() SPC "environments.","success");
	}
}
initEnvDB();

function loadEnvironment(%file) {
	if(!isFile(%file)) {
		gameDebugMessage("Env","Attempted to change the environment, but the env file specified doesn't exist.","error",1);
		return;
	}
	%res = GameModeGuiServer::parseGameModeFile(%file,1);

	EnvGuiServer::getIdxFromFilenames();
	EnvGuiServer::setSimpleMode();

	if(!$EnvGuiServer::SimpleMode) {
		EnvGuiServer::fillAdvancedVarsFromSimple();
		EnvGuiServer::setAdvancedMode();
	}
}