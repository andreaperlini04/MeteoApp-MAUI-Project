var admin = require("firebase-admin");
var serviceAccount = require("./serviceAccountKey.json");

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount)
});

// INCOLLA QUI IL TOKEN PRESO DALLA CONSOLE WEB DI FIREBASE
var registrationToken = "cR99GtfmSCak8GED5giGNX:APA91bFSKZrPt-g8R3tRDi3AXGTXnxMtMlJ1H7SlZwx7P9VhVOLNQr6cThurkFbtHrmjS80VqfDOCoe6T8Dlc6TJ2LkiUVpmqFWuVPB222d_Vaxao9-wjS0";

// Rimuove eventuali spazi o a capo invisibili copiati per sbaglio!
registrationToken = registrationToken.trim(); 

console.log("Sto provando a inviare al Token: " + registrationToken);
console.log("Lunghezza Token: " + registrationToken.length + " caratteri");

var message = {
    token: registrationToken,
    notification: {
        title: "Server Node.js",
        body: "Notifica Ricevuta"
    }
};

admin.messaging().send(message)
.then(function(response) {
    console.log("VITTORIA! Notifica inviata:", response);
})
.catch(function(error) {
    console.log("Errore:", error.errorInfo.message);
});