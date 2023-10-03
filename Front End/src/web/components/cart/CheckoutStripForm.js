import React from 'react';
import { useStripe, useElements, CardElement } from '@stripe/react-stripe-js';

import CardSection from './CardSection';
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import { showErrorMsg, showSuccessMsg } from '../../../helpers/ValidationHelper';
import rootAction from '../../../stateManagment/actions/rootAction';
import { useSelector, useDispatch } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { LOADER_DURATION } from '../../../helpers/Constants';



export default function CheckoutStripForm(props) {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const stripe = useStripe();
  const elements = useElements();

  const handleSubmit = async (event) => {



    //--start loader
    dispatch(rootAction.commonAction.setLoading(true));


    try {
      event.preventDefault();



      if (!stripe || !elements) {
        // Stripe.js has not yet loaded.
        // Make  sure to disable form submission until Stripe.js has loaded.
        return;
      }

      const card = elements.getElement(CardElement);
      const result = await stripe.createToken(card);

      if (result.error) {
        // Show error to your customer.
        showErrorMsg("An error occured. Please try again!");
        console.log(result.error.message);

        //--stop loader
        setTimeout(() => {
          dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);


      } else {
        // Send the token to your server.
        // This function does not exist yet; we will define it in the next step.
        //await  stripeTokenHandler(result.token);

        props.PlaceAndConfirmCustomerOrder(result.token.id);

        dispatch(rootAction.commonAction.setLoading(false));

        return false;

      }
    }
    catch (err) {
      showErrorMsg("An error occured. Please try again!");
      console.log(err.message);
      props.HandleStripCardModal();


      //--stop loader
      setTimeout(() => {
        dispatch(rootAction.commonAction.setLoading(false));
      }, LOADER_DURATION);


    }




  };

  return (
    <form onSubmit={handleSubmit}>
      <CardSection />
      <button disabled={!stripe}>Confirm order</button>
    </form>
  );
}

