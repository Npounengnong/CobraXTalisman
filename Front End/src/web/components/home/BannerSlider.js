import React, { Component } from 'react';
import Link from 'next/link';
import VisibilitySensor from "react-visibility-sensor";
import dynamic from 'next/dynamic';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';

import Config from '../../../helpers/Config';
    


const OwlCarousel = dynamic(import('react-owl-carousel3'));


const options = {
    items: 1,
    loop: true,
    nav: false,
    dots: true,
    animateOut: "slideOutDown",
    animateIn: "slideInDown",
    smartSpeed: 750,
    autoplay: true,
    autoplayHoverPause: true,
    navText: [
        "<i class='fas fa-arrow-left'></i>",
        "<i class='fas fa-arrow-right'></i>"
    ]
}

class BannerSlider extends Component {

    constructor(props) {
        super(props);

        this.state = {
            display: false,
            panel: true,
            bannerList: [],
            adminPanelBaseURL: Config['ADMIN_BASE_URL']
        };
    }





    async componentDidMount() {

         
        const headers = {
            Accept: 'application/json',
            'Content-Type': 'application/json',
               
        }


        const param = {
            requestParameters: {
                recordValueJson: "[]",
            },
        };

            

        //--Get product detail
        const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_HOME_SCREEN_BANNER'], null, param, headers, "POST", true);
        console.log(response);
        if (response != null && response.data != null) {

            this.setState({ bannerList: JSON.parse(response.data.data) });

        }

        setTimeout(() => {
            this.setState({ display: true });
        }, 500);



    }




    render() {
        return (
            <>
                {this.state.display ? <OwlCarousel
                    className="home-slides-three owl-carousel owl-theme"
                    {...options}
                >

                    {
                        this.state.bannerList?.map((item, idx) =>

                            <div className="main-banner" style={{ backgroundImage: `url(${this.state.adminPanelBaseURL + item.BannerImgUrl})` }}>
                                <div className="d-table">
                                    <div className="d-table-cell">
                                        <div className="container">
                                            <VisibilitySensor>
                                                {({ isVisible }) => (
                                                    <div className="main-banner-content white-color">
                                                        <span
                                                            className={
                                                                isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'
                                                            }
                                                        >
                                                            {item.TopTitle}
                                                        </span>
                                                        <h1
                                                            className={
                                                                isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'
                                                            }
                                                        >
                                                             {item.MainTitle}
                                                        </h1>
                                                        <p
                                                            className={
                                                                isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'
                                                            }
                                                        >
                                                              {item.BottomTitle}
                                                        </p>


                                                        {
                                                            item.LeftButtonText != undefined && item.LeftButtonText.length > 0 ?
                                                                <>
                                                                    <Link href="#">
                                                                        <a className={
                                                                            `btn btn-primary ${isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'}`
                                                                        }

                                                                            onClick={(e) => {
                                                                                e.preventDefault();
                                                                                this.props.handleBannerButtonClickUrl(item.LeftButtonUrl);
                                                                            }}
                                                                        >
                                                                            {item.LeftButtonText}
                                                                        </a>
                                                                    </Link>
                                                                </> :
                                                                <>

                                                                </>
                                                        }

                                                        {
                                                            item.RightButtonText != undefined && item.RightButtonText.length > 0 ?
                                                                <>
                                                                    <Link href="#">
                                                                        <a
                                                                            className={
                                                                                `btn btn-light ${isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'}`
                                                                            }

                                                                            onClick={(e) => {
                                                                                e.preventDefault();
                                                                                this.props.handleBannerButtonClickUrl(item.RightButtonUrl);
                                                                            }}
                                                                        >
                                                                            {item.RightButtonText}
                                                                        </a>
                                                                    </Link>
                                                                </> :
                                                                <>

                                                                </>
                                                        }


                                                    </div>
                                                )}
                                            </VisibilitySensor>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}

                    {/* <div className="main-banner item-bg10">
                        <div className="d-table">
                            <div className="d-table-cell">
                                <div className="container">
                                    <VisibilitySensor>
                                        {({ isVisible }) => (
                                            <div className="main-banner-content white-color">
                                                <span
                                                    className={
                                                        isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'
                                                    }
                                                >
                                                    Limited Time Offer!
                                                </span>
                                                <h1
                                                    className={
                                                        isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'
                                                    }
                                                >
                                                    Winter-spring 2020!
                                                </h1>
                                                <p
                                                    className={
                                                        isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'
                                                    }
                                                >
                                                    Take 20% Off ‘Sale Must-Haves'
                                                </p>

                                                <Link href="#">
                                                    <a
                                                        className={
                                                            `btn btn-primary ${isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'}`
                                                        }
                                                    >
                                                        Shop Women's
                                                    </a>
                                                </Link>

                                                <Link href="#">
                                                    <a
                                                        className={
                                                            `btn btn-light ${isVisible ? "animated fadeInUp opacityOne" : 'opacityZero'}`
                                                        }
                                                    >
                                                        Shop Men's
                                                    </a>
                                                </Link>
                                            </div>
                                        )}
                                    </VisibilitySensor>
                                </div>
                            </div>
                        </div>
                    </div> */}


                </OwlCarousel> : ''}
            </>
        );
    }
}

export default BannerSlider;
