import {createStore, applyMiddleware, combineReducers} from 'redux';
import {persistStore, persistReducer} from 'redux-persist';
import {
  createStateSyncMiddleware,
  initMessageListener,
} from "redux-state-sync";
import thunk from 'redux-thunk';
import storage from 'redux-persist/lib/storage';
import autoMergeLevel2 from 'redux-persist/lib/stateReconciler/autoMergeLevel2';
import logger from 'redux-logger';
import userReducer from './reducers/userReducer'
import commonReducer from './reducers/commonReducer'
import cartReducer from './reducers/cartReducer'



const rootReducer = combineReducers({
    userReducer,
    commonReducer,
    cartReducer
});

// Middleware: Redux Persist Config
const persistConfig = {
  // Root
  key: 'root',
  storage: storage,
  whitelist: ['userReducer', 'commonReducer', 'cartReducer'],
  //blacklist: ['modalReducer'],
  stateReconciler: autoMergeLevel2,
  // preloadedState: loadState(),
};
// Middleware: Redux Persist Persisted Reducer
const persistedReducer = persistReducer(persistConfig, rootReducer);
// Redux: Store

const reduxStore = createStore(persistedReducer, applyMiddleware(createStateSyncMiddleware({
  blacklist: ["persist/PERSIST", "persist/REHYDRATE"],
}), thunk));
initMessageListener(reduxStore);


// Middleware: Redux Persist Persister
let persistor = persistStore(reduxStore);
// Exports
export {reduxStore, persistor};
