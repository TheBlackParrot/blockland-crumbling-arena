function MinigameSO::playSound(%this,%data) {
	if(!isObject(%data)) {
		return;
	}
	for(%i=0;%i<%this.numMembers;%i++) {
		%this.member[%i].playSound(%data);
	}
}