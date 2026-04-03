// To run this code, use the command: node sendNotification.js
// install firebase-admin package using npm before running the code: npm install firebase-admin

var admin = require("firebase-admin");
var serviceAccount = require("./serviceAccountKey.json");

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount)
});

// andare a d C:\Users\UTENTE\AppData\Local\Android\Sdk\platform-tools\
// poi fare .\adb.exe logcat | findstr "FCM"
var registrationToken = "c_Yx79FtQXa4pLvG7ViQUc:APA91bHfSh7HQK7vPZm9dyA1k6JXG3yfd_vPU_R-5FbjInxabxtHd0UY5lFGtRcy-Trat3HYnqnTucUtYDc4R86dgNfdCND61X4igZzm6UlY9OP3rMdtg8Q";

var message = {
    token: registrationToken,
    notification: {
        title: "Notification Title",
        body: "This is a notification message"
    }
};

admin.messaging().send(message)
.then(function(response) {
    console.log("Notification sent successfully:", response);
})
.catch(function(error) {
    console.log("Error sending Notification:", error);
});