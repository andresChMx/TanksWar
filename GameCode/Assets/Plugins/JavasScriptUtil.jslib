mergeInto(LibraryManager.library, {
addSocketIO:function(ssl,url,port){
                url="192.168.0.13";
                var socketIOScript = document.createElement('script');
                socketIOScript.setAttribute('src', 'http' + (ssl ? "s" : "") + "://" + url + (!ssl && port != 0 ? ":" + port.toString() : "") +  "/socket.io/socket.io.js");
                document.head.appendChild(socketIOScript);

},
addEventListeners:function(name){
    window.socketEvents = {};

    window.socketEventListener = function(event, data){
        var socketData = {
            socketEvent: event,
            eventData: typeof data === 'undefined' ? '' : JSON.stringify(data)
        };

        SendMessage( gameObject.name , 'InvokeEventCallback', JSON.stringify(socketData));
    };

},
connect:function(ssl,url,port,name){
    //window.socketIO = io.connect("http" + (ssl ? "s" : "") + "://" + url + (!ssl && port != 0 ? ":" + settings.port.toString() : "") + "/");
    window.socketIO = io.connect("http://192.168.0.13:3000");
          
    window.socketIO.on('connect', function(){
        SendMessage( gameObject.name , 'SetSocketID', window.socketIO.io.engine.id);
    });

    for(var socketEvent in window.socketEvents){
        window.socketIO.on(socketEvent, window.socketEvents[socketEvent]);
    }
},
close:function(){
    if(typeof window.socketIO !== 'undefined')
    window.socketIO.disconnect();
},
emit:function(e){
    if(typeof window.socketIO !== 'undefined')
    window.socketIO.emit(e);
},
emit2:function(e,data){
    if(typeof window.socketIO !== 'undefined')
    window.socketIO.emit(e ,data);
},
emit3:function(e,packetID,name){
    if(typeof window.socketIO !== 'undefined'){
        window.socketIO.emit(e, function(data){
            var ackData = {
                packetID: packetID.ToString(),
                data: typeof data === 'undefined' ? '' : JSON.stringify(data)
            };

            SendMessage(name, 'InvokeAck', JSON.stringify(ackData));
        });
    }
},
emit4:function(e,data,packetID,name){
    if(typeof window.socketIO !== 'undefined'){
        window.socketIO.emit(e, data, function(data){
            var ackData = {
                packetID:packetID.ToString(),
                data: typeof data === 'undefined' ? '' : JSON.stringify(data)
            };

            SendMessage(name , 'InvokeAck', JSON.stringify(ackData));
        });
    }
},
on:function(e){
    if(typeof window.socketEvents[e] === 'undefined'){
        window.socketEvents[e] = function(data){
            window.socketEventListener(e, data);
        };

        if(typeof window.socketIO !== 'undefined'){
            window.socketIO.on(e, function(data){
                window.socketEventListener(e, data);
            });
        }
    }
}

});