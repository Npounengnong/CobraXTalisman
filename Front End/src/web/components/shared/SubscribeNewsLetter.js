import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import { showErrorMsg, showSuccessMsg, validateAnyFormField } from '../../../helpers/ValidationHelper';


const SubscribeNewsLetter = () => {
    const [SubscriberEmail, setSubscriberEmail] = useState('');


    const submitSubscribeForm = async (event) => {
        event.preventDefault();

        let isValid = false;
        let validationArray = [];

         //--validation for email
         isValid = validateAnyFormField('Email', SubscriberEmail, 'email', null, 200, true);
         if (isValid == false) {
             validationArray.push({
                 isValid: isValid
             });
         }

           //--check if any field is not valid
        if (validationArray != null && validationArray.length > 0) {

            isValid = false;
            return false;
        } else {
            isValid = true;
        }

        if (isValid) {

    
            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const param = {
                requestParameters: {
                    SubscriberEmail: SubscriberEmail
                },
            };

                
            //--make api call for data operation
            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['INSERT_SUBSCRIBER'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                let detail = JSON.parse(response.data.data);
                if (detail[0].ResponseMsg == "Saved Successfully") {
                    showSuccessMsg("You have successfully subscribed to news channel!");

                    //--Empty form
                    setSubscriberEmail("");
                   

                } else {
                    showErrorMsg ("An error occured. Please try again later!");
                }
            }
        }

    }


    return (
        <>
             <section className="subscribe-area ptb-60">
                <div className="container">
                    <div className="row align-items-center">
                        <div className="col-lg-6">
                            <div className="newsletter-content">
                                <h3>Subscribe to our newsletter</h3>
                                <p>A short sentence describing what someone will receive by subscribing</p>
                            </div>
                        </div>

                        <div className="col-lg-6">
                            <form className="newsletter-form" data-toggle="validator" onSubmit={submitSubscribeForm}>
                                <input 
                                    type="email" 
                                    className="form-control" 
                                    placeholder="Enter your email address" 
                                    name="EMAIL" 
                                    required={true} 
                                    autoComplete="off" 
                                    value={SubscriberEmail}
                                    onChange={(e) => setSubscriberEmail(e.target.value)}
                                />
                                <button type="submit">Subscribe</button>
                                <div id="validator-newsletter" className="form-result"></div>
                            </form>
                        </div>
                    </div>
                </div>
            </section>
        </>
    );

}


export default SubscribeNewsLetter;
