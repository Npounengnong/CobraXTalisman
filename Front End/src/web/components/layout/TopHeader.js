
import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import Wishlist from '../modal/Wishlist';



const TopHeader = () => {
    const [display, setDisplay] = useState(false);


    const handleWishlist = () => {

        setDisplay(!display);

    }

    
    
    

    return (
        <>
            <div className="top-header">
                <div className="container">
                    <div className="row align-items-center">
                        <div className="col-lg-7 col-md-6">
                            <ul className="top-header-nav">
                                <li><Link to="/about">About</Link></li>
                                {/* <li><Link to="/"><a>Our Stores</a></Link></li> */}
                                <li><Link to="/faq">FAQ's</Link></li>
                                <li><Link to="/contact-us">Contact</Link></li>
                            </ul>
                        </div>

                        <div className="col-lg-5 col-md-6">
                            <ul className="top-header-right-nav">
                                <li>
                                    <Link to="#"
                                        data-toggle="modal"
                                        data-target="#shoppingWishlistModal"
                                        onClick={() => handleWishlist()}
                                    >
                                        Wishlist <i className="far fa-heart"></i>
                                    </Link>
                                </li>
                                {/* <li>
                                    <Link to="/compare">
                                        Compare <i className="fas fa-balance-scale"></i>
                                    </Link>
                                </li> */}
                                {/* <li>
                                    <div className="languages-list">
                                        <select>
                                            <option value="1">Eng</option>
                                            <option value="2">Ger</option>
                                            <option value="3">Span</option>
                                        </select>
                                    </div>
                                </li> */}
                            </ul>
                        </div>
                    </div>
                </div>

            </div>

            {display ? <Wishlist handleWishlist={handleWishlist} /> : ''}
        </>
    );

}


export default TopHeader;