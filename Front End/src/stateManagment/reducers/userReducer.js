import * as actionType from '../actionTypes';

const initialState = {
   
  };


const userReducer = (state = initialState, action) => {
    switch(action.type){
        case actionType.SET_USER:
            return {
                ...state,
                user: action.payload,
                loggedIn: true
            }
        case actionType.LOG_OUT_USER:
            return {
                ...state,
                user: {},
                loggedIn: false
            }
        default:
            return state
    }
}

export default userReducer;