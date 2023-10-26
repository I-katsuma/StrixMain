var LibraryStrixWebSocket = {
    $Strix: {},

    StrixConnect: function(id, url) {
        if (typeof Strix.webSockets === "undefined") {
            Strix.webSockets = {};
        }
        
        var urlString = Pointer_stringify(url);
        var webSocket = new WebSocket(urlString);
        webSocket.binaryType = "arraybuffer";
        
        Strix.webSockets[id] = webSocket;

        var connected = false;
        
        webSocket.onopen = function(_openEvent) {
            connected = true;
            Module._strixOnOpen(id, 0, null, false);
        }

        webSocket.onmessage = function(messageEvent) {
            Strix.webSockets[id].receivedData = messageEvent.data;
            Module._strixOnMessage(id, messageEvent.data.byteLength);
        }

        webSocket.onerror = function(errorEvent) {
            if (connected) {
                Module._strixOnError(id);
            } else {
                console.error("Error while connecting to " + urlString + ". Error event: %o", errorEvent);
            }
        }

        webSocket.onclose = function(closeEvent) {
            if (connected) {
                Module._strixOnClose(id, closeEvent.code, allocateUTF8OnStack(closeEvent.reason), closeEvent.wasClean);
                Strix.webSockets[id] = null;
            } else {
                Module._strixOnOpen(id, closeEvent.code, allocateUTF8OnStack(closeEvent.reason), closeEvent.wasClean);
            }
        }

        Strix.webSockets[id] = webSocket;
    },

    StrixClose: function(id, code, reason) {
        var reasonString = Pointer_stringify(reason);
        var webSocket = Strix.webSockets[id];
        webSocket.close(id, code, reasonString);
    },

    StrixGetStatus: function(id) {
        var webSocket = Strix.webSockets[id];
        return webSocket.readyState;
    },

    StrixSend: function(id, bufferPointer, bufferLength) {
        var webSocket = Strix.webSockets[id];
        webSocket.send(Module.HEAPU8.subarray(bufferPointer, bufferPointer + bufferLength));
    },
    
    StrixReceive: function(id, bufferPointer) {
        var webSocket = Strix.webSockets[id];
        Module.HEAPU8.set(new Uint8Array(webSocket.receivedData), bufferPointer);
    }
};

autoAddDeps(LibraryStrixWebSocket, '$Strix');
mergeInto(LibraryManager.library, LibraryStrixWebSocket);
