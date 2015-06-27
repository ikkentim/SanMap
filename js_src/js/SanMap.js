// SanMap.js
// Tool for drawing Google Maps of San Andreas.
// Written by Tim Potze

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

/* Create a set of helper classes.
 */

/** 
 * Projection specialized for San Andreas, based on GallPetersProjection
 * available at:
 * https://developers.google.com/maps/documentation/javascript/examples/map-projection-simple
 * @class SanMapProjection
 * @constructor
 * *implements {google.maps.Projection}
 */
function SanMapProjection(tileSize) {
	/**
     * The range across the map.
	 *
	 * @property projectionRange_
	 * @type {Number}
	 */
    var projectionRange_ = tileSize;
	
	/**
     * The origin of the map.
	 *
	 * @property pixelOrigin_
	 * @type {Object}
	 */
    this.pixelOrigin_ = new google.maps.Point(projectionRange_ / 2, 
		projectionRange_ / 2);
	
	/**
     * The number of pixels per longitude degree.
	 *
	 * @property pixelsPerLonDegree_
	 * @type {Number}
	 */
    this.pixelsPerLonDegree_ = projectionRange_ / 360;
	
	/**
     * Converts a google.maps.LatLng to google.maps.Point.
	 *
	 * @method fromLatLngToPoint
	 * @param {Object} latLng The LatLng object to convert.
	 * @param {Object} opt_point optional point type to use as return type
	 *	instead of google.maps.Point.
	 * @return {Object} The newly created point.
	 */
    this.fromLatLngToPoint = function (latLng, opt_point) {
        var point = opt_point || new google.maps.Point(0, 0);
		
        point.x = this.pixelOrigin_.x + latLng.lng() *
			this.pixelsPerLonDegree_ * 2;
        point.y = this.pixelOrigin_.y - latLng.lat() *
			this.pixelsPerLonDegree_ * 2;
			
        return point;
    }
	
	/**
     * Converts a google.maps.Point to google.maps.LatLng.
	 *
	 * @method fromLatLngToPoint
	 * @param {Object} point The Point object to convert.
	 * @return {Object} The newly created LatLng.
	 */
    this.fromPointToLatLng = function (point) {
        var lng = (point.x - this.pixelOrigin_.x) /
			this.pixelsPerLonDegree_ / 2;
        var lat = (-point.y + this.pixelOrigin_.y) /
			this.pixelsPerLonDegree_ / 2;
			
        return new google.maps.LatLng(lat, lng, true);
    }
};

/**
 * Simple class for providing a google.maps.ImageMapType based on the provided
 * zoom limitations and function for providing tiles.
 * @class SanMapType
 * @constructor
 */
function SanMapType(minZoom, maxZoom, getTileUrl, tileSize) {

	/**
	 * Creates an instance of google.maps.ImageMapType based on the provided
	 * zoom limitations and function for providing tiles.
	 *
	 * @method getImageMapType
	 * @param {Boolean} repeating Whether the map should repeat horizontally.
	 * @return {Object} The newly created ImageMapType.
	 */
    this.getImageMapType = function (repeating) {
		/* Default tileSize to 512.
		 */
		tileSize = tileSize || 512;
		
        return new google.maps.ImageMapType({
            getTileUrl: function (coord, zoom) {
                var x = coord.x, 
					y = coord.y, 
					max = 1 << zoom;

				/* If not repeating and x is outside of the range -or- y is
				 * outside of the range, return a clear tile. This can be
				 * provided by getTileUrl, using the tile coordinates (-1, -1).
				 */
                if (y < 0 || y >= max || 
					(repeating !== true && (x < 0 || x >= max))) {
                    return getTileUrl(zoom, -1, -1);
                }
				
				/*
				 * Return the provided tile. Make sure x is within the 
				 * range 0 - max.
				 */
                return getTileUrl(zoom, (((x % max) + max) % max), y);
            },
            tileSize: new google.maps.Size(tileSize, tileSize),
            maxZoom: maxZoom,
            minZoom: minZoom
        });
    }
};

/* Define a number of SanMap methods.
 */
function SanMap(){ }

/**
 * Creates an instance of google.maps.Map with the provided map types.
 *
 * @method createMap
 * @param {Object} canvas The element to draw the map on.
 * @param {Number} mapTypes The map types available in the map control.
 * @param {Number} zoom The initial zoom level.
 * @param {Object} center The intial center point to focus on.
 * @param {Boolean} repeating Whether the map should repeat horizontally.
 * @param {String} defaultMap The default map type.
 * @return {Object} The newly created Map.
 */
SanMap.createMap = function(canvas, mapTypes, zoom, center, repeating, 
	defaultMap) {
	/* If no mapTypes are parsed, return null and display a warning
	 */
    if (mapTypes === undefined || mapTypes.length == 0) {
		console.warn("SanMap: No map types were parsed with SanMap.createMap.");
        return null;
    }

	/* Create the map
	 */
    var map = new google.maps.Map(canvas,  {
        zoom: zoom || 2,
        center: center || SanMap.getLatLngFromPos(0, 0),
        streetViewControl: false,
        mapTypeControlOptions: {
            mapTypeIds: Object.keys(mapTypes)
        }
    });

	/* Add every map type to the map.
	 */
    for (var key in mapTypes) {
        if (mapTypes.hasOwnProperty(key)) {
            var type = mapTypes[key].getImageMapType(repeating || false);
            type.name = type.alt = key;
			type.projection = new SanMapProjection(type.tileSize.width);
            map.mapTypes.set(key, type);
        }
    }

	/* Set the default map type.
	 */
    map.setMapTypeId(defaultMap || Object.keys(mapTypes)[0]);

	/* If not repeating, bound the viewable area.
	 */
    if (!repeating) {
        bounds = new google.maps.LatLngBounds(new google.maps.LatLng(-90,-90), 
			new google.maps.LatLng(90,90));

		/* When the center changes, check if the new center is within the bounds
		 * of the map. If not, move the center to within these bounds.
		 */
        google.maps.event.addListener(map, 'center_changed', function () {
            if (bounds.contains(map.getCenter()))
                return;

            var lng = map.getCenter().lng(),
                lat = map.getCenter().lat();

            if (lng < bounds.getSouthWest().lng())
				lng = bounds.getSouthWest().lng();
				
            if (lng > bounds.getNorthEast().lng())
				lng = bounds.getNorthEast().lng();
				
            if (lat < bounds.getSouthWest().lat())
				lat = bounds.getSouthWest().lat();
				
            if (lat > bounds.getNorthEast().lat())
				lat = bounds.getNorthEast().lat();
				
            map.setCenter(new google.maps.LatLng(lat, lng));
        });
    }
	
	return map;
};

/* Conversion properties. */
SanMap.width = 6000;
SanMap.height = 6000; 
SanMap.ox = 0;
SanMap.oy = 0; 

/**
 * Set the properties of the map coordinate system.
 *
 * @method setMapSize
 * @param {Number} width The width of the map.
 * @param {Number} y The GTA:SA y-coordinate.
 */
SanMap.setMapSize = function (width, height, offsetx, offsety) {
    SanMap.width = width;
    SanMap.height = height;
    SanMap.ox = offsetx;
    SanMap.oy = offsety;
}

/**
 * Converts a GTA:SA coordinates to an instance of google.maps.LatLng.
 *
 * @method getLatLngFromPos
 * @param {Number} x The GTA:SA x-coordinate.
 * @param {Number} y The GTA:SA y-coordinate.
 * @return {Object} The newly created LatLng.
 */
SanMap.getLatLngFromPos = function (x, y) {
    return typeof(x) == "object" 
		? new google.maps.LatLng((x.y - SanMap.oy) / SanMap.height * 180, (x.x - SanMap.ox) / SanMap.width * 180) 
		: new google.maps.LatLng((y - SanMap.oy) / SanMap.height * 180, (x - SanMap.ox) / SanMap.width * 180);
}

/**
 * Converts an instance of google.maps.LatLng to a GTA:SA coordinates.
 *
 * @method getPosFromLatLng
 * @param {Object} latLng The LatLng to convert..
 * @return {Object} An Object containing the GTA:SA coordinates.
 */
SanMap.getPosFromLatLng = function (latLng) {
    return {x: latLng.lng() * SanMap.width / 180 + SanMap.ox, y: latLng.lat() * SanMap.height / 180 + SanMap.oy};
}
