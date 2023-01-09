# ImageDb

This is a personal program used to manage a folder of images. Images can be added to a folder and tracked in a configuration file.
Duplicate images can be detected before being added to the folder. Images can be marked as used, and an image can be chosen from
the currently unused images.

Program arguments: `<command> [args]`  
Commands:  
`add <file>` Move the image to the image folder and add it to the configuration file.  
`remove <file>` Remove an image from the configuration file.  
`init` Create default configuration files.  
`index <directory>` Equivalent to using `add` on all files in a directory.  
`showjson` Write the content of the configuration files in plain JSON.  
`lookup <file> [tolerance]` Search for a similar looking image in the file tree.  
`insert <file>` Runs `lookup`, then prompts to add. If yes, the image is added, otherwise nothing happens.  
`insertDir <dir> <tolerance>` Runs `insert` on each file in a directory, but will automatically accept a file if the tolerance is greater than or equal to the given value.  
`use <file>` Mark an image as used. Will be `add`ed if it is not in the configuration file.  
`useAll <directory>` This will `use` all files in the directory.  
`removeUse <file>` Manually unmark a file as used.  
`showUsed` Print the list of all used files.  
`choose` Pick an unused image and mark it as used.
