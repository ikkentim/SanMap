To generate the tiles used in the example (../index.html), run the following commands:
SanMap -i "\path\to\reposity\source\tiles\map.png" -z 5
SanMap -i "\path\to\reposity\source\tiles\sat.jpg" -z 5

or to use imagemagic:
SanMap -i "\path\to\reposity\source\tiles\map.png" -z 5 -m
SanMap -i "\path\to\reposity\source\tiles\sat.jpg" -z 5 -m

Using imagemagic generates better results, using GDI(without -m) is quicker.
