# Jellyfin Tagger
Somtimes, we get two episodes in one file, which share the same number.
And sometimes, this won't match up with most metadata providers, as they use numbers for the individual episode halves.
This results in jellyfin giving the episodes the wrong names. From those providers. 
Despite the episodes already having embedded tags with correct information.
This is annoying.
This plugin workaround shall fix this.

## Usage
Place a .forcetags file into the same directory as the episodes to apply the workaround on.
Now jellyfin will extract the episode titles from their embedded MKV tags and apply them to its database.
