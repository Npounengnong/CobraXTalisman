import React , {useState, useEffect} from 'react';
// import LoadingOverlay from 'react-loading-overlay';
import { Spinner } from 'reactstrap';
import {useSelector, useDispatch} from 'react-redux';


const LoadingScreen = () => {
    const loadingState = useSelector(state=>state.commonReducer.loading);
   // const [loadingState, setLoadingState] = useState(false);
  
    return (
            <div  style={{
                    // minWidth:'100%', 
                    // minHeight:'100%', 
                    flexDirection:'column',
                    flexGrow:1,
                    flexShrink:0,
                    position:'fixed',
                    left:0,
                    right:0,
                    bottom:0,
                    zIndex:20000,
                    top:0,
                    backgroundColor:'white',
                    opacity:'60%',
                    justifyContent:'center',
                    alignItems:'center',
                    display: loadingState!=undefined && loadingState!=null && loadingState==true ? 'flex' : 'none',
                }}>
                    <Spinner color='success'/>
                    
            </div> 
    )
}

export default LoadingScreen;
