
import Config from "./Config";

export const RedirectToWhatsAppPage = () => {
    window.open('https://wa.me/923128545494', '_blank');
}



export const GetDefaultCurrencySymbol = () => {
    let DefaultCurrencySymbol = "$";  //--USD is consider as default if there is no setting in appsetting.json file
    DefaultCurrencySymbol = Config.APP_SETTING['DefaultCurrencySymbol'] ?? "$";
    return DefaultCurrencySymbol;


}

export const GetDefaultCurrencyCode = () => {
    let DefaultCurrencyCode = "USD";  //--USD is consider as default if there is no setting in appsetting.json file
    DefaultCurrencyCode = Config.APP_SETTING['DefaultCurrencyCode'] ?? "USD";
    return DefaultCurrencyCode;


}



export const GetTokenForHeader =async () => {

        
    try {
        let Token = "";

        let tokenFromStorage = localStorage.getItem("Token");

        if (tokenFromStorage !== null && tokenFromStorage !== undefined && tokenFromStorage !== "") {

            Token = tokenFromStorage;
        }
        return Token;
    }
    catch (err) {
        console.error(err.message);
        return ""; 
    }

}

export const GetUserIdForHeader =async () => {

        
    try {
        let UserID = "";

        let loginUserDataJson = localStorage.getItem("user");
        const loginUser = JSON.parse(loginUserDataJson ?? "{}");

        if (loginUser != null && loginUser != undefined && loginUser != "") {

            UserID = loginUser.UserID;
        }
        return UserID;
    }
    catch (err) {
        console.error(err.message);
        return ""; 
    }

}



