# ACRender

This repository contains the code of `ACRender`, which has been sitting on my hard disk for over a decade.
It contains a fair amount of code to read the various DAT files from Asheron's Call. Textures, models, palettes, etc.
with the intention to render them. 

This code is by no means a fully functional DAT explorer, although it did work for most simple models, I never got to the
more complex things such as animations. The majority of the code was simply aimed at understanding DAT files, so there's tons
of debug code and all sorts of unknowns. I'm sharing the code to help out anyone who might be interested into peeking into
the DAT files themselves.

The code is in C# and does compile and run (does have a dependency on some managed DirectX libraries. This repository
does not include any of the actual DAT files, obviously.

This utility was updated after Throne of Destiny was released, so hopefully it still works with the latest DATs.