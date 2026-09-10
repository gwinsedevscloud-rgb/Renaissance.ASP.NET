// Offline field store — IndexedDB queue for tablet sync
(function () {
  const DB_NAME = 'medreach-field';
  const DB_VERSION = 1;
  const STORE = 'queue';

  function openDb() {
    return new Promise((resolve, reject) => {
      const req = indexedDB.open(DB_NAME, DB_VERSION);
      req.onupgradeneeded = () => {
        const db = req.result;
        if (!db.objectStoreNames.contains(STORE)) {
          const os = db.createObjectStore(STORE, { keyPath: 'clientRecordId' });
          os.createIndex('type', 'type', { unique: false });
          os.createIndex('status', 'status', { unique: false });
        }
      };
      req.onsuccess = () => resolve(req.result);
      req.onerror = () => reject(req.error);
    });
  }

  window.medreachFieldStore = {
    async put(item) {
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        tx.objectStore(STORE).put(item);
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    },
    async getAll() {
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readonly');
        const req = tx.objectStore(STORE).getAll();
        req.onsuccess = () => resolve(req.result || []);
        req.onerror = () => reject(req.error);
      });
    },
    async getPending(type) {
      const all = await this.getAll();
      return all.filter(x => x.status === 'pending' && (!type || x.type === type));
    },
    async markSynced(clientRecordId, serverId, clientNumber) {
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        const store = tx.objectStore(STORE);
        const getReq = store.get(clientRecordId);
        getReq.onsuccess = () => {
          const row = getReq.result;
          if (!row) {
            resolve(false);
            return;
          }
          row.status = 'synced';
          row.serverId = serverId || null;
          row.clientNumber = clientNumber || row.clientNumber || null;
          row.syncedAtUtc = new Date().toISOString();
          store.put(row);
        };
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    },
    async markFailed(clientRecordId, error) {
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        const store = tx.objectStore(STORE);
        const getReq = store.get(clientRecordId);
        getReq.onsuccess = () => {
          const row = getReq.result;
          if (!row) {
            resolve(false);
            return;
          }
          row.status = 'failed';
          row.lastError = error || 'Sync failed';
          store.put(row);
        };
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    },
    async retryFailed() {
      const all = await this.getAll();
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        const store = tx.objectStore(STORE);
        for (const row of all.filter(x => x.status === 'failed')) {
          row.status = 'pending';
          row.lastError = null;
          store.put(row);
        }
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    },
    async discard(clientRecordId) {
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        tx.objectStore(STORE).delete(clientRecordId);
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    },
    async clearSynced() {
      const all = await this.getAll();
      const db = await openDb();
      return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        const store = tx.objectStore(STORE);
        for (const row of all.filter(x => x.status === 'synced')) {
          store.delete(row.clientRecordId);
        }
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    },
    async exportJson() {
      const all = await this.getAll();
      const blob = new Blob([JSON.stringify(all, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `medreach-field-queue-${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}.json`;
      a.click();
      URL.revokeObjectURL(url);
      return true;
    }
  };

  window.medreachConnectivity = {
    isOnline() {
      return navigator.onLine;
    },
    bind(dotNetRef) {
      const notify = () => dotNetRef.invokeMethodAsync('SetOnline', navigator.onLine);
      window.addEventListener('online', notify);
      window.addEventListener('offline', notify);
      notify();
    }
  };
})();
