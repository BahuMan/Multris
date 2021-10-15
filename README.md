# Multris
A Unity project for a game with tetris-like controls

## Current Status
* refactoring to use statuses that can span multiple frames (mostly done)
* local shared screen multiplayer is possible, 1 player using the keyboard and 1 player using (any) joystick
* local keyboard controls: arrows left/right to move blocks, ctrl/alt to rotate blocks, space to drop
* local joystick controls: left joystick or digipad to move blocks, shoulder buttons to rotate, "A" to drop

## Known Bugs
- All solved! Yay!

## To Do
- more animation when disappearing lines
- blocks above deleted line should glide down rather than teleport
- networked multiplayer (which network lib/protocol? PUN ?)
- menus, named players
- what should happen when a player quits/dies ?
- special blocks
