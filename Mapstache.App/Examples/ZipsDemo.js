var osm = new OpenLayers.Layer.OSM();

var statesUtf8Grid = new OpenLayers.Layer.UTFGrid({
    url: "/examples/Utf8GridZipsHandler.ashx?z=${z}&x=${x}&y=${y}",
    utfgridResolution: 2, // default is 2
    displayInLayerSwitcher: true
});


// add editable layer
var styleMap = new OpenLayers.StyleMap({
    'strokeWidth': 2,
    'strokeColor': '#0000FF',
    'fillColor': '#0000FF',
    'fillOpacity': 0.1
});
var editableVectorLayer = new OpenLayers.Layer.Vector("Editable", { displayInLayerSwitcher: false, styleMap: styleMap });

var statesWmsLayer = new OpenLayers.Layer.WMS("States",
                        "/wms", {
                            layers: 'states',
                            format: 'image/png',
                            transparent: true
                        },
                        {
                            isBaseLayer: true,
                            singleTile: true,
                            displayInLayerSwitcher: true,
                            layername: 'states'
                        }
                    );
                        
var map = new OpenLayers.Map({
    div: "map", 
    projection: "EPSG:900913",
    layers: [osm]
});
var centerLL = new OpenLayers.LonLat(-97,33);
var centerM = centerLL.transform(new OpenLayers.Projection("EPSG:4326"), this.map.getProjectionObject());
map.setCenter(centerM,10);
map.addLayer(statesUtf8Grid);
map.addLayer(editableVectorLayer);
//map.addControl(new OpenLayers.Control.LayerSwitcher());

var callback = function (infoLookup) {
    var msg = "";
    if (infoLookup) {
        var info;
        for (var idx in infoLookup) {
            // idx can be used to retrieve layer from map.layers[idx]
            info = infoLookup[idx];
            if (info && info.data) {
                msg += "[" + info.id + "] ZIP: " +
                    info.data.Zip + "   PO Name: " +
                        info.data.PO_Name;

                var editableLayer = this.map.getLayersByName('Editable')[0];
                editableLayer.removeAllFeatures();
                var wktParser = new OpenLayers.Format.WKT();
                var features = wktParser.read(info.data.Wkt);
                var bounds;
                if (features) {
                    if (features.constructor != Array) {
                        features = [features];
                    }
                    for (var i = 0; i < features.length; ++i) {
                        if (!bounds) {
                            bounds = features[i].geometry.getBounds();
                        } else {
                            bounds.extend(features[i].geometry.getBounds());
                        }
                    }
                    editableLayer.addFeatures(features);
                }
            }
        }

    }
    document.getElementById("attrs").innerHTML = msg;
};
    
var controls = {
    move: new OpenLayers.Control.UTFGrid({
        callback: callback,
        handlerMode: "move"
    }),
    hover: new OpenLayers.Control.UTFGrid({
        callback: callback,
        handlerMode: "hover"
    }),
    click: new OpenLayers.Control.UTFGrid({
        callback: callback,
        handlerMode: "click"
    })
};
for (var key in controls) {
    map.addControl(controls[key]);
}

function toggleControl(el) {
    for (var c in controls) {
        controls[c].deactivate();
    }
    controls[el.value].activate();
}

 function ddnLayer (bounds, layer) {
        var res = layer.map.getResolution();
        var x = Math.round((bounds.left - layer.tileOrigin.lon) / (res * layer.tileSize.w));
        var y = Math.round((bounds.bottom - layer.tileOrigin.lat) / (res * layer.tileSize.h));
        var z = layer.map.getZoom();
        var isBing = layer.map.baseLayer.type == 'Road' || layer.map.baseLayer.type == 'AerialWithLabels' || layer.map.baseLayer.type == 'Aerial';
        if (isBing){
            z = z + 1;
        }
        var path = '/tilehandler.ashx?layer='+layer.layername+'&format=' + layer.type + '&x=' + x + '&y=' + y + '&z=' + z+ '&w=' + '<%= this.MapSettings.MasterId %>';
        var url = layer.url;
        if (OpenLayers.Util.isArray(url)) {
            url = layer.selectUrl(path, url);
        }
        return url + path;
}
// activate the control that responds to mousemove
toggleControl({value: "move"});
