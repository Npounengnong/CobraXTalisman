import React from 'react';
import './web/resources/themeContent/styles/bootstrap.min.css'
import './web/resources/themeContent/styles/fontawesome.min.css'
import './web/resources/themeContent/styles/animate.min.css'
import './web/resources/themeContent/styles/slick.css'
import './web/resources/themeContent/styles/slick-theme.css'
import 'react-toastify/dist/ReactToastify.css';
import 'react-accessible-accordion/dist/fancy-example.css';
import 'react-image-lightbox/style.css';
import './web/resources/themeContent/styles/style.css';
import './web/resources/themeContent/styles/responsive.css';
import RouteConfig from './RouteConfig';

//--Redux related imports starts here
import {Provider} from 'react-redux';
import {PersistGate} from 'redux-persist/integration/react';
import {persistor, reduxStore} from '../src/stateManagment/reduxStore';
//--Redux related imports ends here

function App() {

  return (
    <Provider store={reduxStore}>
      <PersistGate loading={null} persistor={persistor}>

      <RouteConfig/>

      </PersistGate>
    </Provider>

   
  );
}

export default App;
