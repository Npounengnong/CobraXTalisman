//import React from 'react';
import Config from './Config';

export const makeProductShortDescription = (inputString, length) => {

    length = parseInt(length) ?? 50;

    if (inputString !== undefined && inputString !== null && inputString.length > 0) {
        let newString = inputString.length > length ? (inputString.substring(0, length) + '...') : (inputString.substring(0, length))
        return newString;
    } else {
        return "";
    }

}

export const makeAnyStringLengthShort = (inputString, length) => {

    length = parseInt(length) ?? 50;

    if (inputString !== undefined && inputString !== null && inputString.length > 0) {
        let newString = inputString.length > length ? (inputString.substring(0, length) + '...') : (inputString.substring(0, length))
        return newString;
    } else {
        return "";
    }


}


export const replaceWhiteSpacesWithDashSymbolInUrl = (inputString) => {
    if (inputString !== undefined && inputString !== null && inputString.length > 0) {

        //--replace extra space with one space
        let newString = inputString.replace(/\s\s+/g, ' ');

        //--replace space with '-' character
        newString = newString.replace(/\s+/g, '-').toLowerCase();

        //--replace '/' with '-' character
        return newString.replaceAll('/', '-').toLowerCase();

    } else {
        return inputString;
    }
}

export const convertDateToDifferentFormats = (inputDate, format) => {
    let formattedDate = inputDate;

    if (inputDate === undefined || inputDate === null || inputDate === "") {
        return formattedDate;
    }

    if (format === "dd/mm/yyyy") {
        let today = new Date(inputDate);
        let yyyy = today.getFullYear();
        let mm = today.getMonth() + 1; // Months start at 0!
        let dd = today.getDate();

        if (dd < 10) dd = '0' + dd;
        if (mm < 10) mm = '0' + mm;

        formattedDate = dd + '/' + mm + '/' + yyyy;
    } else if (format === "dd-mm-yyyy") {
        let today = new Date(inputDate);
        let yyyy = today.getFullYear();
        let mm = today.getMonth() + 1; // Months start at 0!
        let dd = today.getDate();

        if (dd < 10) dd = '0' + dd;
        if (mm < 10) mm = '0' + mm;

        formattedDate = dd + '-' + mm + '-' + yyyy;
    } else {
        let today = new Date(inputDate);
        let yyyy = today.getFullYear();
        let mm = today.getMonth() + 1; // Months start at 0!
        let dd = today.getDate();

        if (dd < 10) dd = '0' + dd;
        if (mm < 10) mm = '0' + mm;

        formattedDate = dd + '-' + mm + '-' + yyyy;
    }

    return formattedDate;
}


export const makePriceRoundToTwoPlaces = (price) => {
    price = price ?? 0;
    return +(Math.round(price + "e+2") + "e-2");

}

export const setProductDescriptionImagesUrl = (FullDescription) => {

    try {
        if (FullDescription !== undefined && FullDescription !== null && FullDescription !== '') {
            if (FullDescription.includes('<img src="/content/commonImages/')) {
                let adminPanelBaseURL = Config['ADMIN_BASE_URL'];
                let replaceText = '<img src="' + adminPanelBaseURL + "content/commonImages/";
                FullDescription = FullDescription.replace('<img src="/content/commonImages/', replaceText);
            }

        }
    }
    catch (err) {
        console.log(err.message);
    }

    return FullDescription;

}

