# Multris
A Unity project for a game with tetris-like controls

## Current Status
* local shared screen multiplayer is possible, 1 player using the keyboard and 1 player using (any) joystick
* local keyboard controls: arrows left/right to move blocks, ctrl/alt to rotate blocks, space to drop
* local joystick controls: left joystick or digipad to move blocks, shoulder buttons to rotate, "A" to drop

## Known Bugs
- sometimes, a full line does not disappear
- players's falling blocks don't collide with each other (you can move through each other)

## To Do
- more animation when disappearing lines
- networked multiplayer (which network lib/protocol? PUN ?)
- menus, named players, code should handle joining AND quitting
