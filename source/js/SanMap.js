
// Setup the new type of projection
function EuclideanProjection() {
    var EUCLIDEAN_RANGE = 256;
    this.pixelOrigin_ = new google.maps.Point(EUCLIDEAN_RANGE / 2, EUCLIDEAN_RANGE / 2);
    this.pixelsPerLonDegree_ = EUCLIDEAN_RANGE / 360;
    this.pixelsPerLonRadian_ = EUCLIDEAN_RANGE / (2 * Math.PI);
    this.scaleLat = 2;	// Height
    this.scaleLng = 2;	// Width
    this.offsetLat = 0;	// Height
    this.offsetLng = 0;	// Width
};

EuclideanProjection.prototype.fromLatLngToPoint = function (latLng, opt_point) {
    var point = opt_point || new google.maps.Point(0, 0);

    var origin = this.pixelOrigin_;
    point.x = (origin.x + (latLng.lng() + this.offsetLng ) * this.scaleLng * this.pixelsPerLonDegree_);
    point.y = (origin.y + (-1 * latLng.lat() + this.offsetLat ) * this.scaleLat * this.pixelsPerLonDegree_);
    return point;
};

EuclideanProjection.prototype.fromPointToLatLng = function (point) {
    var me = this;

    var origin = me.pixelOrigin_;
    var lng = (((point.x - origin.x) / me.pixelsPerLonDegree_) / this.scaleLng) - this.offsetLng;
    var lat = ((-1 * ( point.y - origin.y) / me.pixelsPerLonDegree_) / this.scaleLat) - this.offsetLat;
    return new google.maps.LatLng(lat, lng, true);
};


//Map Type object
function SanMapType(minZoom, maxZoom, getTileUrl) {
    this.getImageMapType = function () {
        return new google.maps.ImageMapType({
            getTileUrl: function (coord, zoom) {
                if (coord.y < 0 || coord.y >= 1 << zoom || coord.x < 0 || coord.x >= 1 << zoom)
                    return null;

                var coordx = (coord.x % (1 << zoom));
                while (coordx < 0) {
                    coordx += 1 << zoom;
                }
                return getTileUrl(zoom, coordx, coord.y);
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

    var mapTypeNames = new Array();
    for (var key in mapTypes){
        if (mapTypes.hasOwnProperty(key)) {
            mapTypeNames.push(key);
            mapTypes[key].name=key;
            mapTypes[key].alt=key;
            mapTypes[key].projection = new EuclideanProjection();
        }
    }
    var mapOptions = {
        zoom: zoom === undefined ? 2 : zoom,
        center: SanMap.getLatLngFromPos(0,0),
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
            this.map.mapTypes.set(key, type);
        }
    }

    this.map.setMapTypeId(mapTypeNames[0]);
};

SanMap.getLatLngFromPos = function(x, y) {
    return new google.maps.LatLng(x / (2985 + 1) * 90, y / (2985 + 1) * 90);
}
