import React from 'react';

const GoTop = (props) => {

    const [thePosition, setThePosition] = React.useState(false);
    const timeoutRef = React.useRef(null);

    React.useEffect(() => {
        document.addEventListener("scroll", () => {
            if (window.scrollY > 170) {
                setThePosition(true)
            } else {
                setThePosition(false);
            }
        });
    }, [])
    
    const onScrollStep = () => {
        if (window.pageYOffset === 0){
            clearInterval(timeoutRef.current);
        }
        window.scroll(0, window.pageYOffset - props.scrollStepInPx);
    }

    const scrollToTop = () => {
        timeoutRef.current = setInterval(onScrollStep, props.delayInMs);
    }

    const renderGoTopIcon = () => {
        if(thePosition){
            return (
                <div className={`go-top`} onClick={scrollToTop}>
                    <i className="fas fa-arrow-up"></i>
                    <i className="fas fa-arrow-up"></i>
                </div>
            )
        }
    }

    return (
        <>
            {renderGoTopIcon()}
        </>
    )
}

export default GoTop;