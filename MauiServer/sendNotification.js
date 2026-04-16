var admin = require("firebase-admin");
var serviceAccount = require("./serviceAccountKey.json");

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount)
});

// INCOLLA QUI IL TOKEN PRESO DALLA CONSOLE WEB DI FIREBASE
var registrationToken = "dXdSH-9vTaa1IuWphEB5j1:APA91bGu827VosPXhLu6EFypUHQxXbvlFPphVSjHBDcFdy7FZk-PD8swey9ftuxXUJBAKO4_8Uz5_ObYtplJzcJeDQdHjRkWjKckMM3WU8W1NY41cHJBNQA";

// Rimuove eventuali spazi o a capo invisibili copiati per sbaglio!
registrationToken = registrationToken.trim(); 

console.log("Sto provando a inviare al Token: " + registrationToken);
console.log("Lunghezza Token: " + registrationToken.length + " caratteri");

var message = {
    token: registrationToken,
    notification: {
        title: "Test da Node.js",
        body: "Se leggi questo, abbiamo vinto!"
    }
};

admin.messaging().send(message)
.then(function(response) {
    console.log("VITTORIA! Notifica inviata:", response);
})
.catch(function(error) {
    console.log("Errore:", error.errorInfo.message);
});