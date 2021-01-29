var port = process.env.PORT || 3000;
var express=require("express");
var app=express();
var http=require("http").Server(app);

var io = require('socket.io')(http);
var nombresLogin=[];
var sockets={};
var names={};
var colors=[];
var idCounter=0;
app.use(express.static(__dirname+  "/public"));
app.use("/css",express.static(__dirname + "/public/css"));
app.use("/js",express.static(__dirname + "/public/js"));
app.get("/dist/:nom",function(req,res){
    nombresLogin.push(req.params.nom);
    res.sendFile(__dirname + "/public/dist/index.html");
});
app.get("/andres",function(req,res){
    res.sendFile(__dirname + "/public/login.html");
})

io.on('connection', function(socket){
    console.log("alguein se conecto");
    var tmp=nombresLogin.pop();
    if(tmp==undefined){
        tmp="XXXXX";
    }
    sockets[idCounter]=socket;
    sockets[idCounter].isDeath=false;
    names[idCounter]=tmp;
    colors[idCounter]={r:(Math.random()*127),g:(Math.random()*127),b:(Math.random()*127)};
    socket.broadcast.emit("send-notification",{val:names[idCounter]});
    socket.broadcast.emit("spawn",{num:idCounter,name:names[idCounter],r:colors[idCounter].r,g:colors[idCounter].g,b:colors[idCounter].b,isDeath:false});
    for(var i in sockets){

        if(i==idCounter){
            console.log("spawneadno jugador");
            socket.emit("spawn-me",{num:i,name:names[i],r:colors[i].r,g:colors[i].g,b:colors[i].b,isDeath:sockets[i].isDeath});
        }else{
            socket.emit("spawn",{num:i,name:names[i],r:colors[i].r,g:colors[i].g,b:colors[i].b,isDeath:sockets[i].isDeath});
        }
    }
    socket.on("disconnect",function(){
        var index=0;
        for(var i in sockets){
            if(sockets[i]==socket){
                index=i.toString();
                console.log("ELIMINE A UN USURIOA");
                break;
            }
        }
        socket.broadcast.emit("player-disconnected",{"val":index});
        delete sockets[index]
        delete names[index];
        delete colors[index];
        
    })
    socket.on("death",function(data){

    });
    socket.on("move",function(data){
        socket.broadcast.emit("player-move",data);
    });

    socket.on("set-color",function(data){
        console.log("CAMBAR COLOR");
        socket.broadcast.emit("set-new-color",data);
    })
    idCounter++;


    /*CHAT*/
    socket.on("chat-message",function(data){
        socket.broadcast.emit("take-message",data);
    }); 
    /*BULLETS*/
    socket.on("new-bullet",function(data){
        //console.log("New bullet");
        socket.broadcast.emit("spawn-bullet",data);
    })
    socket.on("bullets-update",function(data){
        //console.log(data);
        socket.broadcast.emit("enemy-bullets-update",data);
    })
    socket.on("remove-bullet",function(data){
        console.log("bullet removedd");
        socket.broadcast.emit("remove-enemy-bullet",data);
    });
    /*HEALTH*/
    socket.on("damage",function(data){
        io.emit("enemy-damage",data);
    }); 
    socket.on("someone-death",function(data){
        for(var i in sockets){
            if(i==data.val){
                sockets[i].isDeath=true;
            }
        }
        socket.broadcast.emit("someone-death",data);
    });
});
http.listen(3000,function(){

});
