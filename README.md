## VS OnBuild Extension ##

### Overview ###

The is extension was born out of a need to run IISRESET any time I wanted to build my solution.  The build process kicks off a file copy at the end that puts the dlls and content in to an active website.  Opening up a cmd shell and running IIS is a distraction that pulls me out of IDE.

### Early Alpha Release ###

This is my first VS Extension and, as such, it is changing regularly as I find more elegant ways to do things.  I don't have the unit tests working at this point and there is only one command to attach to the "on build" event.  These will hopefully be attended to in the near future.
