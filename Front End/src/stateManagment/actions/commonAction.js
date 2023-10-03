import * as actionType from '../actionTypes';

const setLoading = (loading) => {
    return {
        type: actionType.SET_LOADING,
        loading
    }
}

const setWebsiteLogo = (webLogo) => {
    return {
        type: actionType.SET_WEBSITE_LOGO,
        payload: webLogo
    }
}

export default {
    setLoading,
    setWebsiteLogo
}