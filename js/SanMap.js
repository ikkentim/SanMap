// SanMap.js
// Tool for drawing Google Maps of San Andreas.
// Written by Tim Potze
//
// This is free and unencumbered software released into the public domain.
//
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
//
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// For more information, please refer to <http://unlicense.org>
//

//Represents the Projection used in SanMap
function SanMapProjection() {
    //The range across the map
    var projectionRange = 512;
    //The origin (0,0) of the map (Middle of the map)
    this.pixelOrigin_ = new google.maps.Point(projectionRange / 2, projectionRange / 2);
    //The number of pixels per longitude degree
    this.pixelsPerLonDegree_ = projectionRange / 360;
    this.scaleLat = 2;	// latitude scale
    this.scaleLng = 2;	// longitude scale
    //Method to convert LatLng to a Point.
    this.fromLatLngToPoint = function (latLng, opt_point) {
        var point = opt_point || new google.maps.Point(0, 0);
        point.x = this.pixelOrigin_.x + latLng.lng() * this.pixelsPerLonDegree_ * this.scaleLng;
        point.y = this.pixelOrigin_.y - latLng.lat() * this.pixelsPerLonDegree_ * this.scaleLat;
        return point;
    }
    //Method to convert Point to LatLng
    this.fromPointToLatLng = function (point) {
        var lng = (point.x - this.pixelOrigin_.x) / this.pixelsPerLonDegree_ / this.scaleLng;
        var lat = (-point.y + this.pixelOrigin_.y) / this.pixelsPerLonDegree_ / this.scaleLat;
        return new google.maps.LatLng(lat, lng, true);
    }
};

//Represents a MapType used in SanMap
function SanMapType(minZoom, maxZoom, getTileUrl) {
    this.getImageMapType = function (repeating) {
        return new google.maps.ImageMapType({
            getTileUrl: function (coord, zoom) {
                var x = coord.x, y = coord.y, max = 1 << zoom;
                //If not repeating and x is outside range or y is outside range, return null
                if (y < 0 || y >= max || (repeating !== true && (x < 0 || x >= max))) {
                    return getTileUrl(zoom, -1, -1);
                }
                //Get tileX within range
                for (; x < 0; x += max);

                return getTileUrl(zoom, x % max, y);
            },
            tileSize: new google.maps.Size(512, 512),//Range of the map
            maxZoom: maxZoom,//Set zoom levels as given
            minZoom: minZoom
        });
    }
};

//Represents a SanMap Map.
function SanMap(canvas, mapTypes, zoom, center, repeating) {
    //If no mapTypes are parsed, don't continue.
    if (mapTypes === undefined || mapTypes.length == 0) {
        return;
    }

    //Create map with given options
    this.map = new google.maps.Map(document.getElementById(canvas), {
        zoom: zoom || 2,//Default zoom level: 2
        center: center || SanMap.getLatLngFromPos(0, 0),//Default center point: Blueberry
        streetViewControl: false,//No StreetView in GTA
        mapTypeControlOptions: {
            mapTypeIds: Object.keys(mapTypes)//Get names from mapTypes keys
        }
    });

    for (var key in mapTypes) {//Iterate trough mapTypes and add them to the map
        if (mapTypes.hasOwnProperty(key)) {
            var type = mapTypes[key].getImageMapType(repeating || false);
            type.name = type.alt = key;//key = name
            type.projection = new SanMapProjection();
            this.map.mapTypes.set(key, type);
        }
    }

    //Set default mapType to first in mayTypes array.
    this.map.setMapTypeId(Object.keys(mapTypes)[0]);

    //If not repeating, bound the viewable area
    if (!repeating) {
        var map = this.map,
            bounds = new google.maps.LatLngBounds(new google.maps.LatLng(-90,-90), new google.maps.LatLng(90,90));

        //When center changed, check if it's within the bounds of the map
        google.maps.event.addListener(map, 'center_changed', function () {
            if (bounds.contains(map.getCenter()))
                return;

            var lng = map.getCenter().lng(),
                lat = map.getCenter().lat();

            //Check if any direction passed bounds
            if (lng < bounds.getSouthWest().lng()) lng = bounds.getSouthWest().lng();
            if (lng > bounds.getNorthEast().lng()) lng = bounds.getNorthEast().lng();
            if (lat < bounds.getSouthWest().lat()) lat = bounds.getSouthWest().lat();
            if (lat > bounds.getNorthEast().lat()) lat = bounds.getNorthEast().lat();
            map.setCenter(new google.maps.LatLng(lat, lng));
        });
    }
};

//Method to convert GTA positions to google.maps.LatLng objects
SanMap.getLatLngFromPos = function (x, y) {
    return typeof(x) == "object" 
		? new google.maps.LatLng(x.y / 3000 * 90, x.x / 3000 * 90) 
		: new google.maps.LatLng(y / 3000 * 90, x / 3000 * 90);
}

//Method to convert google.maps.LatLng objects to GTA positions
SanMap.getPosFromLatLng = function (latLng) {
    return {x: latLng.lng() * 3000 / 90, y: latLng.lat() * 3000 / 90};
}