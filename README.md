# TerrainGenerator
A program that takes a simple input image to create a complex 3d environment.

To create a random terrain using Simplex noise:
Drag "RandomTerrainGenerator" onto an empty object in Unity and click "Run Now".

To create a terrain using an image: 
Drag "GuidedProceduralGenerator" onto an empty object, then make sure to set "Tex" to the image of your choice. 
Make sure that image is set to texture type "advanced" and has the "Read/Write Enabled" button checked.
Define any other variables how you want them to be, then press the "Run Now" button to make Unity create the guided terrain.

NOTE: Both generators can take almost a minute to run. Make sure to give it that time!

Example Input:  http://i.imgur.com/9uXGWQv.png

Example output: http://i.imgur.com/yavDZzS.png

Example when grass and trees turned on: http://i.imgur.com/7ztz7tp.png

(trees by Unity, not code)

Example input: http://i.imgur.com/JmP0laV.png

Example output: http://i.imgur.com/Pe2UdMP.png

Output when not using an image (random generator using Simplex): http://i.imgur.com/jQkDzWZ.png

