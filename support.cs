function MinigameSO::playSound(%this,%data) {
	if(!isObject(%data)) {
		return;
	}
	for(%i=0;%i<%this.numMembers;%i++) {
		%this.member[%i].playSound(%data);
	}
}

// http://forum.blockland.us/index.php?topic=271862.msg8057643#msg8057643
function findItemByName(%item)
{
	for(%i=0;%i<DatablockGroup.getCount();%i++)
	{
		%obj = DatablockGroup.getObject(%i);
		if(%obj.getClassName() $= "ItemData")
			if(strPos(%obj.uiName,%item) >= 0)
				return %obj.getName();
	}
	return -1;
}

function Player::addNewItem(%player,%item)
{
	%client = %player.client;
	if(isObject(%item))
	{
		if(%item.getClassName() !$= "ItemData") return -1;
		%item = %item.getName();
	}
	else
		%item = findItemByName(%item);
	if(!isObject(%item)) return;
	%item = nameToID(%item);
	for(%i = 0; %i < %player.getDatablock().maxTools; %i++)
	{
		%tool = %player.tool[%i];
		if(!isObject(%tool))
		{
			%player.tool[%i] = %item;
			%player.weaponCount++;
			messageClient(%client,'MsgItemPickup','',%i,%item);
			break;
		}
	}
}

package CrumblingExploitFixes {
	function serverCmdDropTool(%this) {}
};
activatePackage(CrumblingExploitFixes);

function gameDebugMessage(%backend,%message,%type,%override) {
	if(!$Crumbling::Debug && !%override) {
		return;
	}

	switch$(%type) {
		case "error":
			%pre = "\c0";
			%sound = "errorSound";
		case "warning":
			%pre = "\c3";
			%sound = "hammerHitSound";
		case "success":
			%pre = "\c2";
		case "info":
			%pre = "\c6";
	}

	messageAll(%sound,"\c6[" @ %backend @ "]" SPC %pre @ %message);
}