# Multris
A Unity project for a game with tetris-like controls

## Current Status
* refactoring to use statuses that can span multiple frames (so destruction of blocks can be animated)
* local shared screen multiplayer is possible, 1 player using the keyboard and 1 player using (any) joystick
* local keyboard controls: arrows left/right to move blocks, ctrl/alt to rotate blocks, space to drop
* local joystick controls: left joystick or digipad to move blocks, shoulder buttons to rotate, "A" to drop

## Known Bugs
- sometimes, a full line does not disappear
- when multiple lines disappear, the above blocks only drop 1 line

## To Do
- more animation when disappearing lines
- networked multiplayer (which network lib/protocol? PUN ?)
- menus, named players
- what should happen when a player quits/dies ?
