import * as actionType from '../actionTypes';

const setUser = (userObj) => {
    return {
        type: actionType.SET_USER,
        payload: userObj
    }
}

const logOutUser = () => {
    return {
        type: actionType.LOG_OUT_USER
    }
}

export default {
    setUser,
    logOutUser
}