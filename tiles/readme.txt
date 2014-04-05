To generate the tiles used in the example (../index.html), run the following commands:
SanMap -i "\path\to\reposity\tiles\map.png" -z 5
SanMap -i "\path\to\reposity\tiles\sat.jpg" -z 5

or to use imagemagick:
SanMap -i "\path\to\reposity\tiles\map.png" -z 5 -m
SanMap -i "\path\to\reposity\tiles\sat.jpg" -z 5 -m

Using imagemagic generates better results, using GDI(without -m) is a bit quicker, but cannot process enormous images.
