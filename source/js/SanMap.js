//SanMap projection
function SanMapProjection() {
    var projectionRange = 256;
    this.pixelOrigin_ = new google.maps.Point(projectionRange / 2, projectionRange / 2);
    this.pixelsPerLonDegree_ = projectionRange / 360;
    this.pixelsPerLonRadian_ = projectionRange / (2 * Math.PI);
    this.scaleLat = 2;
    this.scaleLng = 2;

    this.fromLatLngToPoint = function (latLng, opt_point) {
        var point = opt_point || new google.maps.Point(0, 0);
        point.x = (this.pixelOrigin_.x + latLng.lng() * this.scaleLng * this.pixelsPerLonDegree_);
        point.y = (this.pixelOrigin_.y - latLng.lat() * this.scaleLat * this.pixelsPerLonDegree_);
        return point;
    }

    this.fromPointToLatLng = function (point) {
        var lng = (((point.x - this.pixelOrigin_.x) / this.pixelsPerLonDegree_) / this.scaleLng);
        var lat = ((-( point.y - this.pixelOrigin_.y) / this.pixelsPerLonDegree_) / this.scaleLat);
        return new google.maps.LatLng(lat, lng, true);
    }
};

//Map Type object
function SanMapType(minZoom, maxZoom, getTileUrl) {
    this.getImageMapType = function () {
        return new google.maps.ImageMapType({
            getTileUrl: function (coords, zoom) {
                if (coords.y < 0 || coords.y >= 1 << zoom || coords.x < 0 || coords.x >= 1 << zoom)
                    return null;

                return getTileUrl(zoom, coords.x, coords.y);
            },
            tileSize: new google.maps.Size(256, 256),
            maxZoom: maxZoom,
            minZoom: minZoom,
            opacity: 1.0
        });
    }
};

function SanMap(canvas, mapTypes, zoom, center) {
    this.canvas = canvas;

    if(mapTypes === undefined) {
        return;
    }

    var mapTypeNames = new Array();
    for (var key in mapTypes){
        if (mapTypes.hasOwnProperty(key)) {
            mapTypeNames.push(key);
        }
    }
    var mapOptions = {
        zoom: zoom || 2,
        center: center || SanMap.getLatLngFromPos(0,0),
        streetViewControl: false,
        keyboardShortcuts: false,
        panControl: false,
        mapTypeControlOptions: {
            mapTypeIds: mapTypeNames
        }
    };

    this.map = new google.maps.Map(document.getElementById(canvas), mapOptions);

    for (var key in mapTypes){
        if (mapTypes.hasOwnProperty(key)) {
            var type = mapTypes[key].getImageMapType();
            type.name=key;
            type.alt=key;
            type.projection = new SanMapProjection();
            this.map.mapTypes.set(key, type);
        }
    }

    this.map.setMapTypeId(mapTypeNames[0]);
};

SanMap.getLatLngFromPos = function(x, y) {
    return new google.maps.LatLng(y / 3000 * 90, x / 3000 * 90);
}
