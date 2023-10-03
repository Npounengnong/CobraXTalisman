//import React from 'react';
import { toast } from 'react-toastify';

export const showSuccessMsg = (message) => {
    toast.success(message);
}


export const showErrorMsg = (message) => {
    toast.error(message);
}

export const showInfoMsg = (message) => {
    toast.info(message);
}

export const showWarningMsg = (message) => {
    toast.warn(message);
}

export const validateEmail = (value) => {
    
    var emailRex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if (emailRex.test(value)) {
        return true;
    }
    return false;
};

export const checkIfStringIsEmtpy = (str) => {
    if (str === null || str === undefined || str === "undefined") {
        str = '';
    }
    return /\S+/.test(str);
}

export const validateAnyFormField = (fieldName, fieldValue, fieldType, minLenght, maxLength, isRequired) => {

    let isFieldValid = true;


    if (isRequired ?? true) {
        if (!checkIfStringIsEmtpy(fieldValue)) {
            isFieldValid = false;
            showErrorMsg(`${fieldName} is required`);
            return false;
        }
    }

    if (minLenght !== undefined && minLenght !== null) {
        if (!checkIfStringIsEmtpy(fieldValue)) {
            isFieldValid = false;
            showErrorMsg(`${fieldName} is required`);
            return false;
        }

        if (fieldValue.length < minLenght) {
            isFieldValid = false;
            showErrorMsg(`${fieldName} min length should be ${minLenght} characters`);
            return isFieldValid;
        }
    }

    if (maxLength !== undefined && maxLength !== null) {
        if (!checkIfStringIsEmtpy(fieldValue)) {
            isFieldValid = false;
            showErrorMsg(`${fieldName} is required`);
            return isFieldValid;
        }

        if (fieldValue.length > maxLength) {
            isFieldValid = false;
            showErrorMsg(`${fieldName} max length should be ${maxLength} characters`);
            return isFieldValid;
        }
    }


    switch (fieldType) {
        case 'email':
            if (!validateEmail(fieldValue)) {
                showErrorMsg(`Please enter valid email address`);
                isFieldValid = false;
            }
            break;
        case 'password':

            break;

        case 'number':

            break;

        case 'date':

            break;

        case 'checkbox':

            break;
        default:
            break;
    }

    return isFieldValid;
}