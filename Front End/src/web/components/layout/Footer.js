import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import Config from '../../../helpers/Config';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import { checkIfStringIsEmtpy } from '../../../helpers/ValidationHelper';
    

const Footer = () => {
    const dispatch = useDispatch();
    const [paymentMethods, setPaymentMethods] = useState([]);
    const [adminPanelBaseURL, setadminPanelBaseURL] = useState(Config['ADMIN_BASE_URL']);

    const [LogoImageFromStorage, setLogoImageFromStorage] = useState(useSelector(state => state.commonReducer.websiteLogoInLocalStorage));


    useEffect(() => {
        // declare the data fetching function
        const DataOperationFunc = async () => {

             
            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                   
            }

            const param = {
                requestParameters: {
                    recordValueJson: "[]",
                },
            };


            //--Get payment methods
            const responsePaymentMethods = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_PAYMENT_METHODS'], null, param, headers, "POST", true);
            if (responsePaymentMethods != null && responsePaymentMethods.data != null) {
                await setPaymentMethods(JSON.parse(responsePaymentMethods.data.data));

            }

            //--Get Website Logo
            if (!checkIfStringIsEmtpy(LogoImageFromStorage)) {

                let paramLogo = {
                    requestParameters: {
                        recordValueJson: "[]",
                    },
                };

                let WebsiteLogoInLocalStorage = "";
                let logoResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_WEBSITE_LOGO'], null, paramLogo, headers, "POST", true);
                if (logoResponse != null && logoResponse.data != null) {
                    console.log(logoResponse.data)

                    if(logoResponse.data.data!=""){
                        let logoData = JSON.parse(logoResponse.data.data);
                        WebsiteLogoInLocalStorage = logoData[0].AppConfigValue;
                        dispatch(rootAction.commonAction.setWebsiteLogo(WebsiteLogoInLocalStorage));
                         setLogoImageFromStorage(WebsiteLogoInLocalStorage);
                     }

                  
                }
            }


        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        DataOperationFunc().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);


    }, [])

    return (
        <>
            <footer className="footer-area">
                <div className="container">
                    <div className="row">
                        <div className="col-lg-3 col-md-6">
                            <div className="single-footer-widget">
                                {/* <div className="logo">
                                    <Link to="/">
                                        <img src={ adminPanelBaseURL + LogoImageFromStorage} width={155} height={41} alt="logo" />
                                    </Link>
                                </div> */}

                                <p>Aurel's shop is a Multi Vendors eCommerce Web Application built with the help of ASP MVC .NET 6 and React Js. The Admin/Vendor Panel is build with ASP MVC .NET 6 and Front End (Customer Store) is built with React Js plus .NET 6 Rest APIs.</p>
                            </div>
                        </div>

                        <div className="col-lg-3 col-md-6">
                            <div className="single-footer-widget">
                                <h3>Quick Links</h3>

                                <ul className="quick-links">
                                    <li>
                                        <Link to="/">
                                            Home
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/about">
                                            About Us
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/faq">
                                            Faq's
                                        </Link>
                                    </li>

                                </ul>
                            </div>
                        </div>

                        <div className="col-lg-3 col-md-6">
                            <div className="single-footer-widget">
                                <h3>Information</h3>

                                <ul className="information-links">
                                    <li>
                                        <Link to="/about">
                                            About Us
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/contact-us">
                                            Contact Us
                                        </Link>
                                    </li>

                                </ul>
                            </div>
                        </div>

                        <div className="col-lg-3 col-md-6">
                            <div className="single-footer-widget">
                                <h3>Contact Us</h3>

                                <ul className="footer-contact-info">
                                    <li>
                                        <i className="fas fa-map-marker-alt"></i>
                                        Location: 1600 Pennsylvania Ave NW <br /> District of Colombia, United states
                                    </li>
                                    <li>
                                        <i className="fas fa-phone"></i>
                                        Call Us: <Link to="tel:(+123) 456-7898">(+1)123-456-7890</Link>
                                    </li>
                                    <li>
                                        <i className="far fa-envelope"></i>
                                        Email Us: <Link to="mailto:support@novine.com">aurel.npounengnong@gmail.com</Link>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="copyright-area">
                    <div className="container">
                        <div className="row align-items-center">
                            <div className="col-lg-6 col-md-6">
                                <p>Copyright &copy; 2023 Aurel's ECommerce App. All Rights Reserved</p>
                            </div>

                            <div className="col-lg-6 col-md-6">
                                <ul className="payment-card">

                                    {
                                        paymentMethods?.map((item, idx) =>


                                            <li>
                                                <Link to="#">
                                                <img src={adminPanelBaseURL + item.ImageUrl} alt="image" />
                                                </Link>
                                            </li>

                                        )}


                                   
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </footer>
        </>
    );

}


export default Footer;
