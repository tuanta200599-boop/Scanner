
self.addEventListener('install', event => {
    console.log('Service Worker installed');
});

self.addEventListener('activate', event => {
    console.log('Service Worker activated');
});
importScripts('/firebase-app-compat.js');
importScripts('/firebase-messaging-compat.js');

// Khởi tạo Firebase
firebase.initializeApp({
    apiKey: "AIzaSyC9IgmwPJI80fRjvjO35v3u5Q6zrsm8IM4",
    authDomain: "fir-config-24515.firebaseapp.com",
    projectId: "fir-config-24515",
    storageBucket: "fir-config-24515.appspot.com",
    messagingSenderId: "750710806613",
    appId: "1:750710806613:web:d9c3db08ba1b7b94c89cdc"
});

// Lấy messaging instance
const messaging = firebase.messaging();

// Xử lý thông báo khi ở background
//messaging.onBackgroundMessage(function (payload) {
//    console.log('[firebase-messaging-sw.js] Nhận background message:', payload);

//    const notificationTitle = (payload.notification && payload.notification.title) || "Thông báo";
//    const notificationOptions = {
//        body: payload.notification?.body || 'Không có nội dung'
//        //icon: '/icon.png' // thay nếu cần
//    };

//    self.registration.showNotification(notificationTitle, notificationOptions);
//});
// Khởi tạo BroadcastChannel để gửi tin nhắn cho UI
const bc = new BroadcastChannel('fcm_notifications');

messaging.onBackgroundMessage((payload) => {
    console.log('[firebase-messaging-sw.js] Nhận background message:', payload);

    const notificationTitle =
        payload.notification?.title || payload.data?.title || "Thông báo";

    const notificationOptions = {
        body: payload.notification?.body || payload.data?.body || 'Không có nội dung',
        icon: '/favicon.ico'
    };

    // Gửi dữ liệu cho Foreground UI nếu đang mở
    bc.postMessage({
        type: 'FCM_NOTIFICATION',
        payload: payload
    });
    console.log('--- [firebase-messaging-sw.js] Đã postMessage vào BroadcastChannel');

    self.registration.showNotification(notificationTitle, notificationOptions)
        .then(() => console.log('✅ Notification đã được hiển thị'))
        .catch(err => console.error('❌ Lỗi khi hiển thị notification:', err));
});
