import React from "react";
import {Routes,Route} from "react-router-dom";
import Footer from "./web/components/layout/Footer";
import GoTop from "./web/components/layout/GoTop";
import Navbar from "./web/components/layout/Navbar";
import LoadingScreen from "./web/components/shared/LoadingScreen";
import ContactUs from "./web/views/contactUs/Index";
import Home from "./web/views/home/Index";
import AllProducts from "./web/views/products/AllProducts";
import ProductDetail from "./web/views/products/ProductDetail";

import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import Login from "./web/views/login/Login";
import Signup from "./web/views/signup/Signup";
import Checkout from "./web/views/checkout/Checkout";
import Cart from "./web/views/checkout/Cart";
import About from "./web/views/about/About";
import FaqPage from "./web/views/faq/FaqPage";
import UpdateProfile from "./web/views/signup/UpdateProfile";
import Campaign from "./web/views/campaign/Campaign";
import OrdersHistory from "./web/views/signup/OrdersHistory";
import WhatsappChatButton from "./web/components/shared/WhatsappChatButton";
import ResetPassword from "./web/views/login/ResetPassword";
import Refresh from "./web/views/common/Refresh";




export default function RouteConfig() {

    return (
        <>

            <LoadingScreen />
            <Navbar />

            <Routes>
                <Route path="/" default element={<Home />} />
                <Route path="/all-products/:category_id/:category_name" element={<AllProducts />} />
                <Route path="/product-detail/:product_id/:category/:product_name" default element={<ProductDetail />} />
                <Route path="/contact-us" element={<ContactUs />} />
                <Route path="/login" element={<Login />} />
                <Route path="/signup" element={<Signup />} />
                <Route path="/reset-password" element={<ResetPassword />} />
                <Route path="/checkout" element={<Checkout />} />
                <Route path="/cart" element={<Cart />} />
                <Route path="/about" element={<About />} />
                <Route path="/faq" element={<FaqPage />} />
                <Route path="/update-profile" element={<UpdateProfile />} />
                <Route path="/campaign/:campaign_id/:campaign_main_title" default element={<Campaign />} />
                <Route path="/orders-history" element={<OrdersHistory />} />
                <Route path="/refresh" element={<Refresh />} />
                
                

            </Routes>


            <Footer />
            <GoTop scrollStepInPx="100" delayInMs="10.50" />


         <WhatsappChatButton/>



            <ToastContainer
                position="top-right"
                autoClose={5000}
                hideProgressBar={false}
                newestOnTop={false}
                rtl={false}
                pauseOnFocusLoss
                draggable={true}
                pauseOnHover={true}
                closeOnClick={true}
                theme="colored"
            />




        </>
    );
}
