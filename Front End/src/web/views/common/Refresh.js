import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Helmet } from 'react-helmet';
import Config from '../../../helpers/Config';
import SiteBreadcrumb from '../../components/layout/SiteBreadcrumb';
import BestFacilities from '../../components/shared/BestFacilities';
import about1 from '../../resources/themeContent/images/theme-images/about1.jpg';
import about2 from '../../resources/themeContent/images/theme-images/about2.jpg';
import signature from '../../resources/themeContent/images/theme-images/signature.png';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';



//-- This page is only for refreshing the data like login user, site logo etc
const Refresh = () => {
    const dispatch = useDispatch();

    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);



    useEffect(() => {
        // declare the data fetching function
        const dataOperationInUseEffect = async () => {


            //--Clear user data from redux
            dispatch(rootAction.userAction.setUser("{}"));
            localStorage.setItem("user", "{}");

            //--clear customer cart
            dispatch(rootAction.cartAction.setCustomerCart('[]'));
            dispatch(rootAction.cartAction.SetTotalCartItems(0));
            localStorage.setItem("cartItems", "[]");

            //--Clear Logo
            dispatch(rootAction.commonAction.setWebsiteLogo(""));

            //--stop loader
            setTimeout(() => {
                window.location.href = "/";
            }, LOADER_DURATION);


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
                <title>{siteTitle} - Refresh</title>
                <meta name="description" content={siteTitle + " - Refresh"} />
                <meta name="keywords" content="Refresh"></meta>
            </Helmet>

            <SiteBreadcrumb title="Refresh" />

            <section className="about-area ptb-60">
                <div className="container">
                    <div className="row align-items-center">
                        <div className="col-lg-6 col-md-12">
                            <div className="about-content">

                                <p>All data refreshed successfully.</p>



                            </div>
                        </div>


                    </div>
                </div>
            </section>



            <BestFacilities />

        </>
    );

}

export default Refresh;
