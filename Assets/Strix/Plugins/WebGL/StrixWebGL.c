#include <stdint.h>         /* contains definitions of exact-width integers (int8_t, int16_t, etc.) */
#include "emscripten.h"     /* contains a definition for EMSCRIPTEN_KEEPALIVE */

typedef void (*ConnectionOpenedDelegate) (int32_t, int32_t, void *, int32_t);
typedef void (*MessageReceivedDelegate)  (int32_t, int32_t);
typedef void (*ConnectionErrorDelegate)  (int32_t);
typedef void (*ConnectionClosedDelegate) (int32_t, int32_t, void *, int32_t);

ConnectionOpenedDelegate  _onOpen;
MessageReceivedDelegate   _onMessage;
ConnectionErrorDelegate   _onError;
ConnectionClosedDelegate  _onClose;

/*
* Called from C# to store callback function pointers
* so that they can be invoked from JavaScript later.
*/
void strixSetCallbacks(
    ConnectionOpenedDelegate  onOpen,
    MessageReceivedDelegate   onMessage,
    ConnectionErrorDelegate   onError,
    ConnectionClosedDelegate  onClose
) {
    _onOpen    = onOpen;
    _onMessage = onMessage;
    _onError   = onError;
    _onClose   = onClose;
}

/*
* EMSCRIPTEN_KEEPALIVE macro forces LLVM to not dead-code-eliminate the function.
* It also exports the function so it can be called from JavaScript.
* All exported function are accessible through the Module object,
* but emscripten adds an underscore before a function's name,
* so strixOnOpen() can be called from JavaScript like this:
* 
*    Module._strixOnOpen(...)
*/
void EMSCRIPTEN_KEEPALIVE strixOnOpen    (int32_t id, int32_t closeCode, void *closeReason, int32_t wasClean) { _onOpen    (id, closeCode, closeReason, wasClean); }
void EMSCRIPTEN_KEEPALIVE strixOnMessage (int32_t id, int32_t messageSize)                                    { _onMessage (id, messageSize);                      }
void EMSCRIPTEN_KEEPALIVE strixOnError   (int32_t id)                                                         { _onError   (id);                                   }
void EMSCRIPTEN_KEEPALIVE strixOnClose   (int32_t id, int32_t closeCode, void *closeReason, int32_t wasClean) { _onClose   (id, closeCode, closeReason, wasClean); }