import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import SiteBreadcrumb from '../../components/layout/SiteBreadcrumb';
import BestFacilities from '../../components/shared/BestFacilities';
import { useSelector, useDispatch } from 'react-redux';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { showErrorMsg, showInfoMsg, showSuccessMsg, validateAnyFormField } from '../../../helpers/ValidationHelper';
import { MakeApiCallSynchronous, MakeApiCallAsync, GetLoadSTRPPublishable } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import rootAction from '../../../stateManagment/actions/rootAction';
import OrderSummary from '../../components/cart/OrderSummary';
import { makePriceRoundToTwoPlaces, makeProductShortDescription } from '../../../helpers/ConversionHelper';
import { Helmet } from 'react-helmet';

//--strip
import { Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';

import CheckoutStripForm from '../../components/cart/CheckoutStripForm';
import { GetDefaultCurrencyCode, GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';

//--Paypal
import { PayPalScriptProvider, PayPalButtons } from "@paypal/react-paypal-js";


// Make sure to call `loadStripe` outside of a component's render to avoid
// recreating the `Stripe` object on every render.
const stripePromise = loadStripe(process.env.REACT_APP_STRP_PUBLISHABLE_KEY);



const Checkout = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();



    const [showCardSectionStripe, setshowCardSectionStripe] = useState(false);
    const [showCardSectionPaypal, setshowCardSectionPaypal] = useState(false);
    const [PaymentMethod, setPaymentMethod] = useState(process.env.REACT_APP_STRIPE_PAYMENT_METHOD ?? 5);
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [OrderNote, setOrderNote] = useState('');
    const [CartSubTotal, setCartSubTotal] = useState(0);
    const [ShippingSubTotal, setShippingSubTotal] = useState(0);
    const [OrderTotal, setOrderTotal] = useState(0);
    const [OrderTotalAfterDiscount, setOrderTotalAfterDiscount] = useState(0);
    const [cartProductsData, setCartProductsData] = useState(0);
    const [CouponCode, setCouponCode] = useState('');
    const [IsCouponCodeApplied, setIsCouponCodeApplied] = useState(false);
    const [IsAlreadyDiscountApplied, setIsAlreadyDiscountApplied] = useState(false);
    const [CouponCodeCssClass, setCouponCodeCssClass] = useState('cart-coupon-code');
    const [paypalOrderID, setPaypalOrderID] = useState(false);



    const loginUserDataJson = useSelector(state => state.userReducer.user);
    const loginUser = JSON.parse(loginUserDataJson ?? "{}");

    const cartJsonDataSession = useSelector(state => state.cartReducer.cartItems);
    const cartItemsSession = JSON.parse(cartJsonDataSession ?? "[]");


    if (loginUser == undefined || loginUser.UserID == undefined || loginUser.UserID < 1) {
        navigate('/login');
    }

    if (cartItemsSession == undefined || cartItemsSession == null || cartItemsSession.length < 1) {
        showInfoMsg('Your cart is empty');

        navigate('/');
    }

    const GetCouponCodeInfo = async () => {

        if (IsCouponCodeApplied) {
            showInfoMsg('Coupon code is already applied!');
            return false;
        }


        let isValid = validateAnyFormField('Coupon Code', CouponCode, 'text', null, 30, true);
        if (isValid == false) {
            return false;
        }



        const headersCoupon = {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }

        const paramCoupon = {
            requestParameters: {
                CouponCode: CouponCode,
                cartJsonData: JSON.stringify(cartItemsSession),
            },
        };


        const couponResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_COUPON_CODE_DISCOUNT'], Config['COMMON_CONTROLLER_SUB_URL'], paramCoupon, headersCoupon, "POST", true);


        if (couponResponse != null && couponResponse.data != null) {

            let copounData = JSON.parse(couponResponse.data.data);
            console.log(copounData);
            if (copounData != undefined && copounData.DiscountValueAfterCouponAppliedWithQuantity != undefined && copounData.DiscountValueAfterCouponAppliedWithQuantity > 0) {
                setOrderTotalAfterDiscount((OrderTotal - copounData.DiscountValueAfterCouponAppliedWithQuantity ?? 0));
                setIsCouponCodeApplied(true);
            } else {
                showErrorMsg('Invalid coupon code!');
            }

        }



    }

    const handleCheckoutOnSubmit = async (e) => {
        debugger

        try {
            e.preventDefault();

            //-- First Disable all forms
            setshowCardSectionStripe(false);
            setshowCardSectionPaypal(false);

            if (PaymentMethod === process.env.REACT_APP_STRIPE_PAYMENT_METHOD) {
                setshowCardSectionStripe(true);
            } else if (PaymentMethod === process.env.REACT_APP_PAYPAL_PAYMENT_METHOD) {
                setshowCardSectionPaypal(true);
            }
            else if (PaymentMethod === process.env.REACT_APP_CASH_ON_DELIVERY_PAYMENT_METHOD) {
                let isYes = window.confirm("Do you really want to place order?");
                if (isYes) {

                    //--start loader
                    dispatch(rootAction.commonAction.setLoading(true));


                    PlaceAndConfirmCustomerOrder(null);

                    //--stop loader
                    setTimeout(() => {
                        dispatch(rootAction.commonAction.setLoading(false));
                    }, LOADER_DURATION);

                }
            }
        } catch (err) {
            showErrorMsg("An error occured. Please try again!");
            console.log(err.message);
            if (PaymentMethod === process.env.REACT_APP_STRIPE_PAYMENT_METHOD) {
                HandleStripCardModal();
                HandlePaypalCardModal();
            }

            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);
        }



    }


    const PlaceAndConfirmCustomerOrder = async (StripPaymentToken, payPalOrderConfirmJson = "{}") => {
        debugger
        try {

            const headersStrip = {
                Accept: 'application/json',
                'Content-Type': 'application/json',

            }


            const paramSrip = {
                requestParameters: {
                    UserID: loginUser.UserID,
                    OrderNote: OrderNote,
                    cartJsonData: JSON.stringify(cartItemsSession),
                    CouponCode: IsCouponCodeApplied == true ? CouponCode : "",
                    PaymentMethod: PaymentMethod,
                    paymentToken: StripPaymentToken ?? "",
                    payPalOrderConfirmJson: payPalOrderConfirmJson ?? "",
                    recordValueJson: "[]",
                },
            };


            debugger
            const stripServerResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['POST_CUSTOMER_ORDER'], Config['COMMON_CONTROLLER_SUB_URL'], paramSrip, headersStrip, "POST", true);
            if (stripServerResponse != null && stripServerResponse.data != null && stripServerResponse.status == 200) {
                let stripServerResponseDetail = JSON.parse(stripServerResponse.data.data != undefined && stripServerResponse.data.data != "" ? stripServerResponse.data.data : "[]");

                if (stripServerResponseDetail.length > 0 && stripServerResponseDetail[0].ResponseMsg != undefined && stripServerResponseDetail[0].ResponseMsg == "Order Placed Successfully") {
                    showSuccessMsg("Order Placed Successfully!");




                    setTimeout(function () {
                        navigate('/');

                        //--clear customer cart
                        dispatch(rootAction.cartAction.setCustomerCart('[]'));
                        dispatch(rootAction.cartAction.SetTotalCartItems(0));
                        localStorage.setItem("cartItems", "[]");

                    }, 1000);


                } else {
                    showErrorMsg("An error occured. Please try again!");

                }

            } else {
                showErrorMsg("An error occured. Please try again!");

            }


            if (PaymentMethod === process.env.REACT_APP_STRIPE_PAYMENT_METHOD) {
                HandleStripCardModal();
            } else if (PaymentMethod === process.env.REACT_APP_PAYPAL_PAYMENT_METHOD) {
                HandlePaypalCardModal();
            }





        } catch (err) {
            showErrorMsg("An error occured. Please try again!");
            console.log(err.message);
            if (PaymentMethod === process.env.REACT_APP_STRIPE_PAYMENT_METHOD) {
                HandleStripCardModal();
            }

            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);

        }


    }


    const HandleStripCardModal = () => {
        setshowCardSectionStripe(!showCardSectionStripe);
    }

    const HandlePaypalCardModal = () => {
        setshowCardSectionPaypal(!showCardSectionPaypal);
    }

    // creates a paypal order
    const createOrder = (data, actions) => {
        return actions.order
            .create({
                purchase_units: [
                    {
                        description: "Sunflower",
                        amount: {
                            //currency_code: "USD",
                            currency_code: GetDefaultCurrencyCode(),
                            value: OrderTotalAfterDiscount != undefined && OrderTotalAfterDiscount > 0 ? OrderTotalAfterDiscount : OrderTotal,
                        },
                    },
                ],
                // not needed if a shipping address is actually needed
                application_context: {
                    shipping_preference: "NO_SHIPPING",
                },
            })
            .then((paypalOrderID) => {
                setPaypalOrderID(paypalOrderID);
                return paypalOrderID;
            });
    };

    // check paypal Approval
    const onApprove = (data, actions) => {
        debugger
        return actions.order.capture().then(function (details) {
            const { payer } = details;
            //setSuccess(true); replace with your own

            //-- Set paypal json response if approve
            let JsonDetail = JSON.stringify(details);

            //--start loader
            dispatch(rootAction.commonAction.setLoading(true));

            setTimeout(() => {
                PlaceAndConfirmCustomerOrder(null, JsonDetail);
            }, 500);

            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);


            HandlePaypalCardModal();
        });
    };

    //capture likely error for paypal
    const onError = (data, actions) => {
        HandlePaypalCardModal();
        showErrorMsg("An error occured. Please try again!");
    };


    useEffect(() => {
        // declare the data fetching function
        const dataOperationInUseEffect = async () => {

            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }

            //--Get cart data
            const paramCart = {
                requestParameters: {
                    cartJsonData: JSON.stringify(cartItemsSession),
                    recordValueJson: "[]",
                },
            };


            const customerCartResponse = await MakeApiCallAsync(Config.END_POINT_NAMES["GET_CUSTOMER_CART_ITEMS"], Config['COMMON_CONTROLLER_SUB_URL'], paramCart, headers, "POST", true);
            if (customerCartResponse != null && customerCartResponse.data != null) {

                let finalData = JSON.parse(customerCartResponse.data.data);
                console.log(finalData);

                if (finalData != null) {
                    setTimeout(() => {

                        setCartProductsData(finalData.productsData);
                        setCartSubTotal(finalData.cartSubTotal);
                        setShippingSubTotal(finalData.shippingSubTotal);
                        setOrderTotal(finalData.orderTotal);

                        if (finalData.productsData.length > 0 && finalData.productsData.some(el => el.DiscountedPrice > 0)) {
                            setIsAlreadyDiscountApplied(true);
                        }

                    }, 500);
                }




            }

        }


        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));


        // call the function
        dataOperationInUseEffect().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);

    }, [])


    return (
        <>


            <Helmet>
                <title>{siteTitle} - Checkout</title>
                <meta name="description" content={siteTitle + " - Checkout"} />
                <meta name="keywords" content="Checkout"></meta>
            </Helmet>

            <SiteBreadcrumb title="Checkout" />

            <section className="checkout-area ptb-60">
                <div className="container">
                    {/* <div className="row">
                        <div className="col-lg-12 col-md-12">
                            <div className="user-actions">
                                <i className="fas fa-sign-in-alt"></i>
                                <span>Returning customer? <Link to="#">Click here to login</Link></span>
                            </div>
                        </div>
                    </div> */}

                    <form onSubmit={handleCheckoutOnSubmit}>
                        <div className="row">
                            <div className="col-lg-5 col-md-5">
                                <div className="billing-details">
                                    <h3 className="title">Billing Details</h3>

                                    <div className="row">
                                        <div className="col-lg-12 col-md-12">
                                            <div className="form-group">
                                                <label>Country</label>
                                                <input
                                                    type="text"
                                                    name="CountryName"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.CountryName}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-6 col-md-6">
                                            <div className="form-group">
                                                <label>First Name</label>
                                                <input
                                                    type="text"
                                                    name="FirstName"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.FirstName}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-6 col-md-6">
                                            <div className="form-group">
                                                <label>Last Name </label>
                                                <input
                                                    type="text"
                                                    name="LastName"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.LastName}
                                                />

                                            </div>
                                        </div>



                                        <div className="col-lg-12 col-md-6">
                                            <div className="form-group">
                                                <label>Shipping Address </label>
                                                <input
                                                    type="text"
                                                    name="address"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.AddressLineOne}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-12 col-md-6">
                                            <div className="form-group">
                                                <label> City </label>
                                                <input
                                                    type="text"
                                                    name="city"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.CityName}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-6 col-md-6">
                                            <div className="form-group">
                                                <label>State / Province </label>
                                                <input
                                                    type="text"
                                                    name="state"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.StateName}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-6 col-md-6">
                                            <div className="form-group">
                                                <label>Postcode / Zip </label>
                                                <input
                                                    type="text"
                                                    name="zip"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.PostalCode}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-6 col-md-6">
                                            <div className="form-group">
                                                <label>Email Address </label>
                                                <input
                                                    type="email"
                                                    name="email"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.EmailAddress}
                                                />

                                            </div>
                                        </div>

                                        <div className="col-lg-6 col-md-6">
                                            <div className="form-group">
                                                <label>Phone </label>
                                                <input
                                                    type="text"
                                                    name="phone"
                                                    className="form-control"
                                                    readOnly
                                                    value={loginUser.MobileNo}
                                                />

                                            </div>
                                        </div>




                                        <div className="col-lg-12 col-md-12">
                                            <div className="form-group">
                                                <textarea name="OrderNote" id="OrderNote" cols="30" rows="6" placeholder="Order Notes" className="form-control"
                                                    value={OrderNote}
                                                    onChange={(e) => setOrderNote(e.target.value)}
                                                />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div className="col-lg-7 col-md-7">
                                <div className="order-details">
                                    <h3 className="title">Your Order</h3>


                                    <>
                                        {
                                            cartProductsData != undefined && cartProductsData != null && cartProductsData.length > 0
                                                ?
                                                <>
                                                    <div className="order-table table-responsive">
                                                        <table className="table table-bordered">
                                                            <thead>
                                                                <tr>
                                                                    <th scope="col">Product Name</th>
                                                                    <th scope="col">Price</th>
                                                                    <th scope="col">Quantity</th>
                                                                    <th scope="col">Total</th>
                                                                </tr>
                                                            </thead>

                                                            <tbody>
                                                                {cartProductsData?.map((data, idx) => (

                                                                    <tr key={idx}>
                                                                        <td className="product-name">
                                                                            <Link to="#">

                                                                                {
                                                                                    makeProductShortDescription(data.ProductName, 50)
                                                                                }
                                                                            </Link>
                                                                        </td>

                                                                        <td className="product-name">
                                                                            <span className="unit-amount">
                                                                                {data.DiscountedPrice != undefined && data.DiscountedPrice > 0 ?
                                                                                    <>
                                                                                        <del style={{ color: "#9494b9" }}>{GetDefaultCurrencySymbol()}{makePriceRoundToTwoPlaces(data.Price)}</del> &nbsp; {GetDefaultCurrencySymbol()}{makePriceRoundToTwoPlaces(data.DiscountedPrice)}
                                                                                    </>
                                                                                    :
                                                                                    <>
                                                                                        {GetDefaultCurrencySymbol()}{makePriceRoundToTwoPlaces(data.Price)}
                                                                                    </>
                                                                                }
                                                                            </span>
                                                                        </td>

                                                                        <td className="product-name">
                                                                            {data.Quantity}
                                                                        </td>

                                                                        {(() => {

                                                                            let itemSubTotal = (data.DiscountedPrice != undefined && data.DiscountedPrice > 0 ? data.DiscountedPrice : data.Price) * (data.Quantity ?? 1);

                                                                            return (

                                                                                <td className="product-total">
                                                                                    <span className="subtotal-amount">${makePriceRoundToTwoPlaces(itemSubTotal)}</span>
                                                                                </td>
                                                                            );

                                                                        })()}


                                                                    </tr>
                                                                ))}

                                                                <tr>
                                                                    <td className="order-subtotal" colSpan={3}>
                                                                        <span>Cart Subtotal</span>
                                                                    </td>


                                                                    <td className="order-subtotal-price">
                                                                        <span className="order-subtotal-amount">{GetDefaultCurrencySymbol()}{makePriceRoundToTwoPlaces(CartSubTotal)}</span>
                                                                    </td>
                                                                </tr>

                                                                <tr>
                                                                    <td className="order-shipping" colSpan={3}>
                                                                        <span>Shipping</span>
                                                                    </td>


                                                                    <td className="shipping-price">
                                                                        <span>${makePriceRoundToTwoPlaces(ShippingSubTotal)}</span>
                                                                    </td>
                                                                </tr>

                                                                <tr>
                                                                    <td className="total-price" colSpan={3}>
                                                                        <span>Order Total</span>
                                                                    </td>



                                                                    <td className="product-subtotal">
                                                                        <span className="subtotal-amount">
                                                                            {OrderTotalAfterDiscount != undefined && OrderTotalAfterDiscount > 0
                                                                                ?
                                                                                <>
                                                                                    <del>{GetDefaultCurrencySymbol()} {makePriceRoundToTwoPlaces(OrderTotal)}</del>&nbsp; &nbsp; {GetDefaultCurrencySymbol()}{makePriceRoundToTwoPlaces(OrderTotalAfterDiscount)}
                                                                                </>



                                                                                :
                                                                                `${GetDefaultCurrencySymbol()} ${makePriceRoundToTwoPlaces(OrderTotal)}`
                                                                            }

                                                                        </span>
                                                                    </td>
                                                                </tr>


                                                                <tr style={{ display: IsAlreadyDiscountApplied ? "none" : '' }}>
                                                                    <td className="total-price" colSpan={3}>
                                                                        <div className='login-form'>
                                                                            <div className='form-group' style={{ marginBottom: '0' }}>
                                                                                <input
                                                                                    type="text"
                                                                                    name="phone"
                                                                                    className={`form-control ${IsCouponCodeApplied ? CouponCodeCssClass : ''}`}
                                                                                    placeholder='Enter Coupon Code'
                                                                                    value={CouponCode}
                                                                                    onChange={(e) => setCouponCode(e.target.value)}
                                                                                    maxLength={30}
                                                                                />

                                                                            </div>

                                                                        </div>

                                                                    </td>



                                                                    <td className="product-subtotal">
                                                                        <button className='btn btn-primary'
                                                                            onClick={(e) => {
                                                                                e.preventDefault();
                                                                                GetCouponCodeInfo();
                                                                            }}

                                                                        >
                                                                            Apply Coupon
                                                                        </button>
                                                                    </td>
                                                                </tr>

                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </>
                                                :
                                                <>
                                                </>
                                        }

                                    </>

                                    <div className="payment-method">
                                        {/* <p>
                                            <input type="radio" id="direct-bank-transfer" name="radio-group" defaultChecked={true} />
                                            <label htmlFor="direct-bank-transfer">Direct Bank Transfer</label>

                                            Make your payment directly into our bank account. Please use your Order ID as the payment reference. Your order will not be shipped until the funds have cleared in our account.
                                        </p>
                                        <p>
                                            <input type="radio" id="paypal" name="radio-group" />
                                            <label htmlFor="paypal">PayPal</label>
                                        </p>
                                        <p>
                                            <input type="radio" id="cash-on-delivery" name="radio-group" />
                                            <label htmlFor="cash-on-delivery">Cash on Delivery</label>
                                        </p> */}


                                        <div className='row login-form'>
                                            <div className="col-lg-6 col-md-6">
                                                <div className="form-group">
                                                    <label>Payment Method</label>


                                                    <select className="form-control" name="PaymentMethod"
                                                        value={PaymentMethod}
                                                        onChange={(e) => setPaymentMethod(e.target.value)}
                                                    >
                                                        <option value="5" selected>Credit Card</option>
                                                        <option value="2" selected>PayPal</option>
                                                        <option value="6">Cash on delivery</option>
                                                    </select>



                                                </div>
                                            </div>

                                        </div>

                                    </div>

                                    {/* <Payment
                                        amount={totalAmount * 100}
                                        disabled={disable}
                                    /> */}

                                    <>
                                        <button type='submit' className='btn btn-primary'>
                                            Place Order
                                        </button>

                                    </>


                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </section>


            <BestFacilities />

            {/*Stripe card section starts here */}
            {
                showCardSectionStripe == true
                    ?


                    <div className="modal fade productQuickView show" style={{ paddingRight: '16px', display: 'block' }}>

                        <div className="modal-dialog modal-dialog-centered" role="document">
                            <div className="modal-content">
                                <button type="button" className="close" data-dismiss="modal" aria-label="Close"
                                    onClick={(e) => {
                                        e.preventDefault();
                                        HandleStripCardModal(e);
                                    }}
                                >
                                    <span aria-hidden="true"><i className="fas fa-times"></i></span>
                                </button>
                                <div className="row align-items-center">


                                    <div className="col-lg-12 col-md-12">
                                        <div className="product-content">
                                            <Elements stripe={stripePromise}>
                                                <CheckoutStripForm

                                                    UserID={loginUser.UserID}
                                                    OrderNote={OrderNote}
                                                    cartJsonData={JSON.stringify(cartItemsSession)}
                                                    ShippingSubTotal={ShippingSubTotal}
                                                    OrderTotal={OrderTotal}
                                                    OrderTotalAfterDiscount={OrderTotalAfterDiscount}
                                                    CouponCode={CouponCode}
                                                    HandleStripCardModal={HandleStripCardModal}
                                                    PlaceAndConfirmCustomerOrder={PlaceAndConfirmCustomerOrder}

                                                />
                                            </Elements>

                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>


                    :
                    <>
                    </>
            }

            {/* Stripe card section ends here */}

            {/* Paypal card section starts here */}

            {
                showCardSectionPaypal == true
                    ?

                    <PayPalScriptProvider
                        options={{
                            "client-id": process.env.REACT_APP_PAYPAL_CLIENT_ID,
                        }}
                    >
                        <div className="modal fade productQuickView show" style={{ paddingRight: '16px', display: 'block' }}>

                            <div className="modal-dialog modal-dialog-centered" role="document">
                                <div className="modal-content">
                                    <button type="button" className="close" data-dismiss="modal" aria-label="Close"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            HandlePaypalCardModal(e);
                                        }}
                                    >
                                        <span aria-hidden="true"><i className="fas fa-times"></i></span>
                                    </button>
                                    <div className="row align-items-center">


                                        <div className="col-lg-12 col-md-12">
                                            <div className="product-content">
                                                <PayPalButtons
                                                    style={{ layout: "vertical" }}
                                                    createOrder={createOrder}
                                                    onApprove={onApprove}
                                                />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </PayPalScriptProvider>

                    :
                    <>
                    </>


            }


            {/* Paypal card section ends here */}


        </>
    );
}

export default Checkout;
