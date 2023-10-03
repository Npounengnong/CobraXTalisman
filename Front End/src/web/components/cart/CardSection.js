/**
* Use the CSS tab above to style your Element's container.
*/
import React from 'react';
import {CardElement} from '@stripe/react-stripe-js';

import  '../../resources/themeContent/styles/stripe-style.css'

const CARD_ELEMENT_OPTIONS = {
    hidePostalCode: true,
   
  style: {
    base: {
    //   color: "#32325d",
    //   fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
    //   fontSmoothing: "antialiased",
    //   fontSize: "16px",
    //   "::placeholder": {
    //     color: "#aab7c4",
    //   },

    iconColor: '#666EE8',
    color: '#31325F',
    lineHeight: '40px',
    fontWeight: 300,
    fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
    fontSize: '15px',

    },
    invalid: {
      color: "#fa755a",
      iconColor: "#fa755a",
    },
  },
};
function CardSection() {

  return (
    <>
      Card details 
      <CardElement options={CARD_ELEMENT_OPTIONS} />
    </>
  );
};
export default CardSection;