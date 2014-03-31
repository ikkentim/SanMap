//Represents the Projection used in SanMap
function SanMapProjection() {
    //The range across the map
    var projectionRange = 512;
    //The origin (0,0) of the map (Middle of the map)
    this.pixelOrigin_ = new google.maps.Point(projectionRange / 2, projectionRange / 2);
    //The number of pixels per longitude degree
    this.pixelsPerLonDegree_ = projectionRange / 360;
    //Method to convert LatLng to a Point.
    this.fromLatLngToPoint = function (latLng, opt_point) {
        var point = opt_point || new google.maps.Point(0, 0);
        point.x = this.pixelOrigin_.x + latLng.lng() * this.pixelsPerLonDegree_;
        point.y = this.pixelOrigin_.y - latLng.lat() * this.pixelsPerLonDegree_;
        return point;
    }
    //Method to convert Point to LatLng
    this.fromPointToLatLng = function (point) {
        var lng = (point.x - this.pixelOrigin_.x) / this.pixelsPerLonDegree_;
        var lat = -( point.y - this.pixelOrigin_.y) / this.pixelsPerLonDegree_;
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
                    return null;
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

    this.map.setMapTypeId(Object.keys(mapTypes)[0]);
};

SanMap.getLatLngFromPos = function (x, y) {
    return new google.maps.LatLng(y / 3000 * 180, x / 3000 * 180);
}