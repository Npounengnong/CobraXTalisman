import * as actionType from '../actionTypes';

const initialState = {
    loading: false
    // tigger:false
};

const commonReducer = (state = initialState, action) => {
    switch (action.type) {
        case actionType.SET_LOADING:
            return {
                ...state,
                loading: action.loading,
            }

        case actionType.SET_WEBSITE_LOGO:
            return {
                ...state,
                websiteLogoInLocalStorage: action.payload

            }

        default:
            return state
    }
}

export default commonReducer;