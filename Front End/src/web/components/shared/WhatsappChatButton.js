import React , {useState, useEffect} from 'react';
import { BrowserRouter, Routes,Link, Route, Navigate } from "react-router-dom";
import { RedirectToWhatsAppPage } from '../../../helpers/CommonHelper';
import whatsAppLogo from '../../resources/themeContent/images/common/whatsapp-logo.png';

const WhatsappChatButton = () => {

  
    return (
           <>
           
           <Link
                to="#!"
                className="whatsapp_float"
                onClick={(e) => {
                    e.preventDefault();
                    RedirectToWhatsAppPage();
                }}
            >
               <img src={whatsAppLogo}/>

            </Link>

           </>
    )
}

export default WhatsappChatButton;
